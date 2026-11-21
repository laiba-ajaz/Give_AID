// Invite Friends page: referral link copy/share, recipient chips and invitation form.
(function () {
  function initInvitePage() {
    const $ = (id) => document.getElementById(id);
    const MAX = 20;

    // ---------- Referral link ----------
    const referral = $('referral-input');
    const copyBtn = $('copy-link-btn');
    if (referral && copyBtn) {
      copyBtn.addEventListener('click', async () => {
        referral.select();
        try {
          await navigator.clipboard.writeText(referral.value);
        } catch {
          document.execCommand('copy');
        }
        const copyBtnText = $('copy-btn-text');
        if (copyBtnText) copyBtnText.textContent = 'Copied!';
        setTimeout(() => {
          if (copyBtnText) copyBtnText.textContent = 'Copy Link';
        }, 2200);
      });

      const shareText = 'Join me in supporting verified NGO programmes on Give-AID:';
      const link = encodeURIComponent(referral.value || '');
      const text = encodeURIComponent(shareText);
      const shareUrls = {
        whatsapp: 'https://wa.me/?text=' + text + '%20' + link,
        linkedin: 'https://www.linkedin.com/sharing/share-offsite/?url=' + link,
        x: 'https://twitter.com/intent/tweet?text=' + text + '&url=' + link,
        telegram: 'https://t.me/share/url?url=' + link + '&text=' + text,
        email: 'mailto:?subject=' + encodeURIComponent('Join me on Give-AID') + '&body=' + text + '%20' + link,
      };
      document.querySelectorAll('[data-share]').forEach((a) => {
        const shareType = a.dataset.share;
        if (shareType && shareUrls[shareType]) {
          a.href = shareUrls[shareType];
        }
      });
    }

    // ---------- Recipients ----------
    const input = $('email-field');
    const errorMsg = $('email-error-msg');
    const container = $('email-tags-container');
    const empty = $('empty-recipients');
    const submitBtn = $('submit-invites-btn');
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

    const staged = () => (container ? [...container.querySelectorAll('.email-chip')] : []);

    function updateStagedCount() {
      const n = staged().length;
      const countEl = $('recipient-count');
      if (countEl) {
        countEl.textContent = n + ' invitation' + (n === 1 ? '' : 's') + ' staged (Up to ' + MAX + ' at a time)';
      }
      const sendLabel = $('send-btn-label');
      if (sendLabel) {
        sendLabel.textContent = 'Send invitations (' + n + ')';
      }
      if (submitBtn) {
        submitBtn.classList.toggle('opacity-50', n === 0);
        submitBtn.classList.toggle('pointer-events-none', n === 0);
      }
      if (empty) {
        empty.classList.toggle('hidden', n > 0);
      }
    }

    function showError(msg) {
      if (errorMsg) {
        errorMsg.textContent = msg || '';
        errorMsg.classList.toggle('hidden', !msg);
      }
      if (input) {
        if (msg) {
          input.classList.add('ring-2', 'ring-red-500');
          input.focus();
        } else {
          input.classList.remove('ring-2', 'ring-red-500');
        }
      }
    }

    function addRecipientEmail() {
      if (!input || !container) return;
      const email = input.value.trim().toLowerCase();

      showError('');

      if (!email) {
        return showError('Please type an email address first.');
      }
      if (!emailRegex.test(email)) {
        return showError('Please provide a valid email format (e.g. name@example.com).');
      }
      if (staged().some((c) => c.dataset.email === email)) {
        return showError('This email is already added.');
      }
      if (staged().length >= MAX) {
        return showError('You can invite up to ' + MAX + ' people at a time.');
      }

      // Built with DOM APIs so typed text can never inject markup
      const chip = document.createElement('div');
      chip.className = 'email-chip inline-flex items-center gap-space-xs bg-surface-container-lowest px-space-sm py-1.5 rounded-full shadow-sm text-primary font-body-sm text-body-sm border border-outline-variant/40';
      chip.dataset.email = email;

      const icon = document.createElement('span');
      icon.className = 'material-symbols-outlined text-[16px] text-tertiary';
      icon.textContent = 'account_circle';

      const label = document.createElement('span');
      label.className = 'font-semibold text-primary';
      label.textContent = email;

      const hidden = document.createElement('input');
      hidden.type = 'hidden';
      hidden.name = 'Emails'; // binds to List<string> Emails on the backend
      hidden.value = email;

      const remove = document.createElement('button');
      remove.type = 'button';
      remove.setAttribute('aria-label', 'Remove ' + email);
      remove.className = 'w-5 h-5 rounded-full hover:bg-surface-container-high flex items-center justify-center text-outline hover:text-red-500 transition-colors ml-1';
      remove.innerHTML = '<span class="material-symbols-outlined text-[14px]">close</span>';
      remove.addEventListener('click', () => {
        chip.remove();
        updateStagedCount();
      });

      chip.append(icon, label, hidden, remove);
      container.appendChild(chip);

      input.value = '';
      input.classList.remove('ring-2', 'ring-red-500');
      input.focus();
      updateStagedCount();
    }

    // Expose globally so inline onclick also works reliably
    window.addRecipientEmail = addRecipientEmail;

    const addBtn = $('add-email-btn');
    if (addBtn) {
      addBtn.addEventListener('click', (e) => {
        e.preventDefault();
        addRecipientEmail();
      });
    }

    if (input) {
      input.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' || e.key === ',') {
          e.preventDefault();
          addRecipientEmail();
        }
      });
      input.addEventListener('input', () => {
        showError('');
      });
    }

    // ---------- Send ----------
    const form = $('inviteForm');
    const successPanel = $('dispatch-success-panel');

    if (form) {
      form.addEventListener('submit', (e) => {
        // If the user typed an email in the field but didn't click Add, add it automatically!
        if (input && input.value.trim().length > 0) {
          addRecipientEmail();
        }

        const n = staged().length;
        if (n === 0) {
          e.preventDefault();
          return showError('Please add at least one recipient email address.');
        }

        // Submit form to InviteController (POST /Invite/Index)
        if (submitBtn) {
          submitBtn.classList.add('opacity-50', 'pointer-events-none');
        }
        const sendLabel = $('send-btn-label');
        if (sendLabel) {
          sendLabel.textContent = 'Sending...';
        }
      });
    }

    const inviteMoreBtn = $('invite-more-btn');
    if (inviteMoreBtn && successPanel) {
      inviteMoreBtn.addEventListener('click', () => {
        successPanel.classList.add('hidden');
        successPanel.classList.remove('flex');
        if (input) input.focus();
      });
    }

    updateStagedCount();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initInvitePage);
  } else {
    initInvitePage();
  }
})();
