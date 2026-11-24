// Register / Log In page: tab switching, password strength, show/hide password, form validation.
(function () {
  const $ = (id) => document.getElementById(id);

  // ---------- Tabs ----------
  const ACTIVE = 'px-space-md py-2 rounded-lg font-title-sm text-title-sm transition-all bg-primary text-on-primary shadow-sm flex items-center gap-2';
  const IDLE = 'px-space-md py-2 rounded-lg font-title-sm text-title-sm transition-all text-on-surface-variant hover:text-primary flex items-center gap-2';
  const tabRegister = $('tabRegister'), tabLogin = $('tabLogin');
  const registerForm = $('registerForm'), loginForm = $('loginForm');

  function setMode(login) {
    tabLogin.className = login ? ACTIVE : IDLE;
    tabRegister.className = login ? IDLE : ACTIVE;
    registerForm.classList.toggle('hidden', login);
    loginForm.classList.toggle('hidden', !login);
    $('switchPrompt').textContent = login ? "Don't have an account? " : 'Already have an account? ';
    $('switchLinkText').textContent = login ? 'Register here' : 'Log In here';
    $('switchLink').setAttribute('href', login ? '#register' : '#login');
    document.title = (login ? 'Log In' : 'Register') + ' | Give-AID';
  }

  tabRegister.addEventListener('click', () => setMode(false));
  tabLogin.addEventListener('click', () => setMode(true));
  $('switchLink').addEventListener('click', (e) => {
    e.preventDefault();
    setMode(loginForm.classList.contains('hidden'));
  });
  // /Account/Register#login or /Account/Login opens the login tab directly.
  if (location.hash === '#login' || /\/login$/i.test(location.pathname)) setMode(true);

  // ---------- Show / hide password ----------
  document.querySelectorAll('.pw-toggle').forEach((btn) => {
    btn.addEventListener('click', () => {
      const input = $(btn.dataset.target);
      const show = input.type === 'password';
      input.type = show ? 'text' : 'password';
      btn.textContent = show ? 'visibility_off' : 'visibility';
      btn.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
    });
  });

  // ---------- Password strength ----------
  const password = $('password'), confirm = $('confirmPassword');
  const bars = $('pwStrengthBars').children;
  const levels = [
    { text: 'Not set', color: '', label: 'text-outline' },
    { text: 'Weak', color: 'bg-error', label: 'text-error' },
    { text: 'Fair', color: 'bg-amber-500', label: 'text-amber-700' },
    { text: 'Good', color: 'bg-emerald-500', label: 'text-emerald-700' },
    { text: 'Strong', color: 'bg-emerald-600', label: 'text-emerald-700' },
  ];

  function score(v) {
    if (!v) return 0;
    let s = 0;
    if (v.length >= 8) s++;
    if (/[A-Z]/.test(v) && /[a-z]/.test(v)) s++;
    if (/\d/.test(v)) s++;
    if (/[^A-Za-z0-9]/.test(v)) s++;
    return Math.max(1, s);
  }

  password.addEventListener('input', () => {
    const lvl = levels[score(password.value)];
    const n = score(password.value);
    [...bars].forEach((b, i) => {
      b.className = 'h-full w-1/4 rounded-full ' + (i < n ? lvl.color : '');
    });
    $('pwStrengthText').textContent = lvl.text;
    $('pwStrengthLabel').className = 'font-semibold flex items-center gap-1 ' + lvl.label;
    if (confirm.value) checkConfirm();
  });

  // ---------- Validation ----------
  const errBox = $('registerError'), errText = $('registerErrorText'), confirmIcon = $('confirmIcon');

  function showError(html) {
    errText.innerHTML = html;
    errBox.classList.toggle('hidden', !html);
    errBox.classList.toggle('flex', !!html);
  }

  function checkConfirm() {
    const mismatch = confirm.value && confirm.value !== password.value;
    confirm.classList.toggle('bg-error-container/30', !!mismatch);
    confirmIcon.classList.toggle('hidden', !confirm.value);
    confirmIcon.textContent = mismatch ? 'error' : 'check_circle';
    confirmIcon.classList.toggle('text-error', !!mismatch);
    confirmIcon.classList.toggle('text-emerald-600', !mismatch);
    if (mismatch) showError('<strong>Mismatch detected:</strong> Passwords must match exactly.');
    else showError('');
    return !mismatch;
  }
  confirm.addEventListener('input', checkConfirm);

  // Organisation tick appears once filled.
  const org = $('orgName');
  org.addEventListener('input', () => $('orgCheck').classList.toggle('hidden', !org.value.trim()));

  const emailOk = (v) => /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(v.trim());

  registerForm.addEventListener('submit', (e) => {
    const problems = [];
    if ($('fullName').value.trim().length < 2) problems.push('full name');
    if (!emailOk($('emailAddress').value)) problems.push('a valid email');
    const phone = $('phoneNum').value.replace(/\D/g, '');
    if (phone && (phone.length < 7 || phone.length > 15)) problems.push('a valid phone number');
    if (password.value.length < 8 || !/[^A-Za-z0-9]/.test(password.value)) problems.push('a password of at least 8 characters with 1 symbol');

    let msg = '';
    if (problems.length) msg = '<strong>Please provide:</strong> ' + problems.join(', ') + '.';
    else if (confirm.value !== password.value) msg = '<strong>Mismatch detected:</strong> Passwords must match exactly.';
    else if (!$('acceptTerms').checked) msg = '<strong>Almost there:</strong> please accept the terms to continue.';

    if (msg) {
      e.preventDefault();
      showError(msg);
      errBox.scrollIntoView({ behavior: 'smooth', block: 'center' });
      return;
    }
    showError('');
  });

  loginForm.addEventListener('submit', (e) => {
    const ok = emailOk($('loginEmail').value) && $('loginPassword').value.length > 0;
    const box = $('loginError');
    box.classList.toggle('hidden', ok);
    box.classList.toggle('flex', !ok);
    if (!ok) { e.preventDefault(); return; }
    
  });
})();
