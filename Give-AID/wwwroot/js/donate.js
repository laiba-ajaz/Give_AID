// Donate page: cause / amount selection, live summary, card validation and success modal.
(function initDonationJourney() {
  const $ = (id) => document.getElementById(id);
  const money = (n) => '$' + n.toFixed(2);

  let currentBaseAmount = 50;
  let coverFees = true;

  const causeButtons = document.querySelectorAll('.cause-btn');
  const amountButtons = document.querySelectorAll('.amount-btn');
  const payTypeButtons = document.querySelectorAll('.pay-type-btn');
  const customInput = $('customAmountInput');
  const feeCheckbox = $('coverFeeCheckbox');
  const freqOnce = $('freqOnce');
  const freqMonthly = $('freqMonthly');
  const form = $('donationForm');

  // Hidden form fields (sent to the backend)
  const fCause = $('fCause'), fAmount = $('fAmount'), fFrequency = $('fFrequency'),
        fCoverFees = $('fCoverFees'), fTotal = $('fTotal'), fCardType = $('fCardType');

  // Summary elements
  const summaryHeadline = $('summaryHeadline');
  const summaryCategory = $('summaryCategory');
  const summaryImpact = $('summaryImpactStatement');
  const summaryFrequency = $('summaryFrequency');

  // ---------- Summary ledger ----------
  function updateLedger() {
    const fee = coverFees ? currentBaseAmount * 0.03 : 0;
    const total = currentBaseAmount + fee;

    $('summaryBaseAmount').textContent = money(currentBaseAmount);
    $('summaryFeeAmount').textContent = money(fee);
    $('feeLabelAmount').textContent = money(currentBaseAmount * 0.03);
    $('summaryTotal').textContent = money(total);
    $('matchBase').textContent = money(currentBaseAmount);
    $('matchTotal').textContent = money(currentBaseAmount * 2);
    $('modalMatch').textContent = money(currentBaseAmount);
    $('ctaButtonLabel').textContent = 'Complete My ' + money(total) + ' Donation';
    $('modalAmount').textContent = money(total) + ' donation';

    fAmount.value = currentBaseAmount.toFixed(2);
    fCoverFees.value = String(coverFees);
    fTotal.value = total.toFixed(2);
  }

  // Generic "select one" styling used by the cause, amount and payment-type groups.
  function selectInGroup(buttons, active) {
    buttons.forEach((b) => {
      b.classList.remove('bg-secondary/10');
      b.classList.add('bg-surface-container-low');
      const chk = b.querySelector('.check-icon');
      if (chk) chk.classList.add('hidden');
    });
    if (!active) return;
    active.classList.add('bg-secondary/10');
    active.classList.remove('bg-surface-container-low');
    const chk = active.querySelector('.check-icon');
    if (chk) chk.classList.remove('hidden');
  }

  // ---------- Cause ----------
  function selectCause(btn) {
    selectInGroup(causeButtons, btn);
    summaryHeadline.textContent = btn.dataset.cause;
    summaryCategory.textContent = btn.dataset.partner;
    // Causes ab database se aate hain: un ki description aur photo bhi summary mein dikhao
    if (btn.dataset.description) summaryImpact.textContent = btn.dataset.description;
    if (btn.dataset.image && $('summaryImage')) $('summaryImage').src = btn.dataset.image;
    $('modalCause').textContent = btn.dataset.cause;
    fCause.value = btn.dataset.key;
  }
  causeButtons.forEach((btn) => btn.addEventListener('click', () => selectCause(btn)));

  // Pre-select the cause from the URL, e.g. /Donate?cause=3 (used by Home page cards; key = cause id).
  const urlCause = new URLSearchParams(location.search).get('cause');
  const preset = urlCause && document.querySelector('.cause-btn[data-key="' + CSS.escape(urlCause.toLowerCase()) + '"]');
  if (preset) selectCause(preset);

  // ---------- Amount ----------
  amountButtons.forEach((btn) => {
    btn.addEventListener('click', () => {
      customInput.value = '';
      selectInGroup(amountButtons, btn);
      currentBaseAmount = parseFloat(btn.dataset.amount);
      summaryImpact.textContent = btn.dataset.impact;
      updateLedger();
    });
  });

  customInput.addEventListener('input', (e) => {
    const val = parseFloat(e.target.value);
    if (!isNaN(val) && val > 0) {
      selectInGroup(amountButtons, null);
      currentBaseAmount = val;
      summaryImpact.textContent = 'Direct flexible emergency funding allocated to ground field workers.';
      updateLedger();
    }
  });

  feeCheckbox.addEventListener('change', (e) => {
    coverFees = e.target.checked;
    updateLedger();
  });

  // ---------- Frequency ----------
  function setFrequency(monthly) {
    const on = monthly ? freqMonthly : freqOnce;
    const off = monthly ? freqOnce : freqMonthly;
    on.classList.add('bg-primary', 'text-on-primary', 'font-bold');
    on.classList.remove('text-on-surface-variant');
    off.classList.remove('bg-primary', 'text-on-primary', 'font-bold');
    off.classList.add('text-on-surface-variant');
    summaryFrequency.textContent = monthly ? 'Monthly Recurring Gift' : 'One-Time Gift';
    fFrequency.value = monthly ? 'Monthly' : 'OneTime';
  }
  freqOnce.addEventListener('click', () => setFrequency(false));
  freqMonthly.addEventListener('click', () => setFrequency(true));

  // ---------- Card type ----------
  payTypeButtons.forEach((btn) => {
    btn.addEventListener('click', () => {
      payTypeButtons.forEach((b) => {
        b.classList.remove('bg-secondary/10', 'text-primary', 'font-semibold', 'shadow-sm');
        b.classList.add('bg-surface-container-low', 'text-on-surface-variant');
        b.querySelector('.material-symbols-outlined').classList.remove('text-secondary');
      });
      btn.classList.add('bg-secondary/10', 'text-primary', 'font-semibold', 'shadow-sm');
      btn.classList.remove('bg-surface-container-low', 'text-on-surface-variant');
      btn.querySelector('.material-symbols-outlined').classList.add('text-secondary');
      fCardType.value = btn.dataset.type;
    });
  });

  // ---------- Card validation ----------
  const cardName = $('cardName'), cardNumber = $('cardNumber'),
        cardExpiry = $('cardExpiry'), cardCvv = $('cardCvv'), cardBrand = $('cardBrand');

  function detectBrand(digits) {
    if (/^4/.test(digits)) return 'VISA';
    if (/^(5[1-5]|2[2-7])/.test(digits)) return 'MC';
    if (/^3[47]/.test(digits)) return 'AMEX';
    if (/^6(011|5)/.test(digits)) return 'DISC';
    return 'CARD';
  }

  // Luhn checksum — rejects mistyped card numbers.
  function luhn(digits) {
    let sum = 0, dbl = false;
    for (let i = digits.length - 1; i >= 0; i--) {
      let d = +digits[i];
      if (dbl) { d *= 2; if (d > 9) d -= 9; }
      sum += d; dbl = !dbl;
    }
    return sum % 10 === 0;
  }

  const validators = {
    cardName(v) {
      v = v.trim();
      if (!v) return 'Please enter the name on the card.';
      if (!/^[A-Za-z][A-Za-z .'-]{1,49}$/.test(v)) return 'Use letters only (2–50 characters).';
      return '';
    },
    cardNumber(v) {
      const d = v.replace(/\D/g, '');
      if (!d) return 'Please enter your card number.';
      if (d.length < 13 || d.length > 19) return 'Card number must be 13–19 digits.';
      if (!luhn(d)) return 'This card number is not valid.';
      return '';
    },
    cardExpiry(v) {
      const m = /^(\d{2})\/(\d{2})$/.exec(v);
      if (!m) return 'Use the format MM/YY.';
      const month = +m[1], year = 2000 + +m[2];
      if (month < 1 || month > 12) return 'Month must be between 01 and 12.';
      const now = new Date();
      const endOfMonth = new Date(year, month, 0, 23, 59, 59);
      if (endOfMonth < now) return 'This card has expired.';
      if (year > now.getFullYear() + 20) return 'Please check the expiry year.';
      return '';
    },
    cardCvv(v) {
      const len = detectBrand(cardNumber.value.replace(/\D/g, '')) === 'AMEX' ? 4 : 3;
      if (!new RegExp('^\\d{' + len + '}$').test(v)) return 'Security code must be ' + len + ' digits.';
      return '';
    },
  };

  function showFieldState(input, message) {
    const wrap = input.closest('.flex-col');
    const err = wrap.querySelector('.field-error');
    const icon = wrap.querySelector('.valid-icon');
    const tag = wrap.querySelector('.valid-tag');
    const ok = !message;

    err.textContent = message;
    err.classList.toggle('hidden', ok);
    input.classList.toggle('ring-2', !ok);
    input.classList.toggle('ring-error/40', !ok);
    input.classList.toggle('bg-error-container/30', !ok);
    input.setAttribute('aria-invalid', String(!ok));
    if (icon) icon.classList.toggle('hidden', !ok);
    if (tag) { tag.classList.toggle('hidden', !ok); tag.classList.toggle('flex', ok); }
    return ok;
  }

  const validate = (input) => showFieldState(input, validators[input.id](input.value));

  // Input masks
  cardNumber.addEventListener('input', () => {
    const d = cardNumber.value.replace(/\D/g, '').slice(0, 19);
    cardNumber.value = d.replace(/(.{4})/g, '$1 ').trim();
    const brand = detectBrand(d);
    cardBrand.textContent = brand;
    cardCvv.maxLength = brand === 'AMEX' ? 4 : 3;
    $('cvvHint').textContent = brand === 'AMEX' ? '4 digits' : '3 digits';
  });
  cardExpiry.addEventListener('input', (e) => {
    let d = cardExpiry.value.replace(/\D/g, '').slice(0, 4);
    if (d.length > 2 || (d.length === 2 && e.inputType !== 'deleteContentBackward')) d = d.slice(0, 2) + '/' + d.slice(2);
    cardExpiry.value = d;
  });
  cardCvv.addEventListener('input', () => { cardCvv.value = cardCvv.value.replace(/\D/g, ''); });

  // Validate on blur, and re-validate while typing once a field has shown an error.
  [cardName, cardNumber, cardExpiry, cardCvv].forEach((input) => {
    input.addEventListener('blur', () => { if (input.value) validate(input); });
    input.addEventListener('input', () => { if (input.getAttribute('aria-invalid') === 'true') validate(input); });
  });

  form.addEventListener('submit', (e) => {
    const results = [cardName, cardNumber, cardExpiry, cardCvv].map(validate);
    let ok = results.every(Boolean);

    if (!(currentBaseAmount >= 1)) {
      ok = false;
      customInput.focus();
    }
    if (!ok) {
      e.preventDefault();
      const firstBad = form.querySelector('[aria-invalid="true"]');
      if (firstBad) firstBad.focus();
      return;
    }

    // Form validated successfully - allow standard POST submission to DonateController.
  });

  // ---------- Success modal ----------
  const successModal = $('successModal');
  const modalCard = $('modalCard');

  window.openSuccessModal = function () {
    const first = cardName.value.trim().split(/\s+/)[0];
    $('modalDonorName').textContent = first ? ', ' + first : '';
    $('modalTxnId').textContent = 'TXN-GIVE-' + new Date().getFullYear() + '-' + Math.floor(100000 + Math.random() * 900000);
    successModal.classList.remove('hidden');
    setTimeout(() => {
      successModal.classList.remove('opacity-0');
      modalCard.classList.remove('scale-95');
      modalCard.classList.add('scale-100');
    }, 10);
  };

  window.closeSuccessModal = function () {
    successModal.classList.add('opacity-0');
    modalCard.classList.remove('scale-100');
    modalCard.classList.add('scale-95');
    setTimeout(() => successModal.classList.add('hidden'), 300);
  };

  updateLedger();
})();
