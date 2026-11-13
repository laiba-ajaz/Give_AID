// Invite Friends page: referral link copy/share, recipient chips and invitation form.
(function () {
  const $ = (id) => document.getElementById(id);
  const MAX = 20;

  // ---------- Referral link ----------
  const referral = $('referral-input');
  $('copy-link-btn').addEventListener('click', async () => {
    referral.select();
    try { await navigator.clipboard.writeText(referral.value); } catch { document.execCommand('copy'); }
    $('copy-btn-text').textContent = 'Copied!';
    setTimeout(() => { $('copy-btn-text').textContent = 'Copy Link'; }, 2200);
  });

  const shareText = 'Join me in supporting verified NGO programmes on Give-AID:';
  const link = encodeURIComponent(referral.value);
  const text = encodeURIComponent(shareText);
  const shareUrls = {
    whatsapp: 'https://wa.me/?text=' + text + '%20' + link,
    linkedin: 'https://www.linkedin.com/sharing/share-offsite/?url=' + link,
    x: 'https://twitter.com/intent/tweet?text=' + text + '&url=' + link,
    telegram: 'https://t.me/share/url?url=' + link + '&text=' + text,
    email: 'mailto:?subject=' + encodeURIComponent('Join me on Give-AID') + '&body=' + text + '%20' + link,
  };
  document.querySelectorAll('[data-share]').forEach((a) => { a.href = shareUrls[a.dataset.share]; });

  // ---------- Recipients ----------
  const input = $('email-field');
  const errorMsg = $('email-error-msg');
  const container = $('email-tags-container');
  const empty = $('empty-recipients');
  const submitBtn = $('submit-invites-btn');
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

  const staged = () => [...container.querySelectorAll('.email-chip')];

  function updateStagedCount() {
    const n = staged().length;
    $('recipient-count').textContent = n + ' invitation' + (n === 1 ? '' : 's') + ' staged (Up to ' + MAX + ' at a time)';
    $('send-btn-label').textContent = 'Send invitations (' + n + ')';
    submitBtn.classList.toggle('opacity-50', n === 0);
    submitBtn.classList.toggle('pointer-events-none', n === 0);
    empty.classList.toggle('hidden', n > 0);
  }

  function showError(msg) {
    errorMsg.textContent = msg;
    errorMsg.classList.toggle('hidden', !msg);
  }

  function addRecipientEmail() {
    const email = input.value.trim().toLowerCase();
    if (!emailRegex.test(email)) return showError('Please provide a valid email format.');
    if (staged().some((c) => c.dataset.email === email)) return showError('This email is already added.');
    if (staged().length >= MAX) return showError('You can invite up to ' + MAX + ' people at a time.');
    showError('');

    // Built with DOM APIs (not innerHTML) so typed text can never inject markup.
    const chip = document.createElement('div');
    chip.className = 'email-chip inline-flex items-center gap-space-xs bg-surface-container-lowest px-space-sm py-1.5 rounded-full shadow-sm text-primary font-body-sm text-body-sm';
    chip.dataset.email = email;

    const icon = document.createElement('span');
    icon.className = 'material-symbols-outlined text-[16px] text-tertiary';
    icon.textContent = 'account_circle';

    const label = document.createElement('span');
    label.textContent = email;

    const hidden = document.createElement('input');
    hidden.type = 'hidden';
    hidden.name = 'Emails';            // binds to List<string> Emails on the backend
    hidden.value = email;

    const remove = document.createElement('button');
    remove.type = 'button';
    remove.setAttribute('aria-label', 'Remove ' + email);
    remove.className = 'w-5 h-5 rounded-full hover:bg-surface-container-high flex items-center justify-center text-outline hover:text-error transition-colors ml-1';
    remove.innerHTML = '<span class="material-symbols-outlined text-[14px]">close</span>';
    remove.addEventListener('click', () => { chip.remove(); updateStagedCount(); });

    chip.append(icon, label, hidden, remove);
    container.appendChild(chip);
    input.value = '';
    input.focus();
    updateStagedCount();
  }

  $('add-email-btn').addEventListener('click', addRecipientEmail);
  input.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' || e.key === ',') { e.preventDefault(); addRecipientEmail(); }
  });

  // ---------- Send ----------
  const form = $('inviteForm');
  const successPanel = $('dispatch-success-panel');

  form.addEventListener('submit', (e) => {
    const n = staged().length;
    if (n === 0) { e.preventDefault(); return showError('Add at least one email address.'); }

    // TODO (backend): remove the lines below to post the form to InviteController (Emails + Message).
    e.preventDefault();
    $('dispatch-title').textContent = n + ' Invitation' + (n === 1 ? '' : 's') + ' Dispatched!';
    successPanel.classList.remove('hidden');
    successPanel.classList.add('flex');
    successPanel.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    successPanel.classList.add('ring-2', 'ring-primary');
    setTimeout(() => successPanel.classList.remove('ring-2', 'ring-primary'), 1500);
    staged().forEach((c) => c.remove());
    updateStagedCount();
  });

  $('invite-more-btn').addEventListener('click', () => {
    successPanel.classList.add('hidden');
    successPanel.classList.remove('flex');
    input.focus();
  });

  updateStagedCount();
})();
