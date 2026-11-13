// Programmes page: category filter + search, card selection and the "Register Your Interest" form.
(function () {
  const $ = (id) => document.getElementById(id);
  const cards = [...document.querySelectorAll('.programme-card')];
  const chips = [...document.querySelectorAll('.filter-chip')];
  const search = $('programmeSearch');
  const ribbon = $('selectedRibbon');

  // ---------- Filtering ----------
  const CHIP_ON = ['bg-primary', 'text-on-primary', 'shadow-sm'];
  const CHIP_OFF = ['bg-surface-container-low', 'hover:bg-surface-container', 'text-on-surface-variant', 'hover:text-on-surface'];
  let activeFilter = 'all';

  function applyFilters() {
    const q = search.value.trim().toLowerCase();
    let shown = 0;
    cards.forEach((card) => {
      const okCat = activeFilter === 'all' || card.dataset.category === activeFilter;
      const okText = !q || card.textContent.toLowerCase().includes(q);
      const visible = okCat && okText;
      card.classList.toggle('hidden', !visible);
      if (visible) shown++;
    });
    $('shownCount').textContent = shown ? '1 – ' + shown : '0';
  }

  function setChip(filter) {
    activeFilter = filter;
    chips.forEach((c) => {
      const on = c.dataset.filter === filter;
      CHIP_ON.forEach((k) => c.classList.toggle(k, on));
      CHIP_OFF.forEach((k) => c.classList.toggle(k, !on));
    });
    applyFilters();
  }

  chips.forEach((c) => c.addEventListener('click', () => setChip(c.dataset.filter)));
  search.addEventListener('input', applyFilters);
  $('resetFilters').addEventListener('click', () => { search.value = ''; setChip('all'); });

  // ---------- Card selection ----------
  function selectCard(card) {
    cards.forEach((c) => {
      c.classList.add('relative');
      c.classList.remove('ring-2', 'ring-secondary/30', 'shadow-md');
      c.classList.add('shadow-sm');
    });
    card.classList.add('ring-2', 'ring-secondary/30', 'shadow-md');
    card.classList.remove('shadow-sm');
    card.prepend(ribbon);

    $('selTitle').textContent = card.querySelector('.programme-title').textContent.trim();
    $('selMeta').textContent = card.dataset.meta;
    $('selThumb').src = card.dataset.thumb;
    $('selProgrammeId').value = card.dataset.id;
    $('interestSuccess').classList.add('hidden');
  }

  cards.forEach((card) => {
    card.querySelector('.interest-btn').addEventListener('click', () => {
      selectCard(card);
      // On small screens the form sits below the list — bring it into view.
      if (window.innerWidth < 1024) $('interestPanel').scrollIntoView({ behavior: 'smooth', block: 'start' });
      $('piName').focus({ preventScroll: window.innerWidth >= 1024 });
    });
  });

  // ---------- Interest form ----------
  const form = $('interestForm');
  const err = $('interestError');
  const success = $('interestSuccess');
  const nameInput = $('piName');

  nameInput.addEventListener('input', () => {
    nameInput.parentElement.querySelector('.valid-icon').classList.toggle('hidden', nameInput.value.trim().length < 2);
  });

  $('clearInterest').addEventListener('click', () => {
    form.reset();
    err.classList.add('hidden');
    success.classList.add('hidden');
    nameInput.parentElement.querySelector('.valid-icon').classList.add('hidden');
  });

  form.addEventListener('submit', (e) => {
    const problems = [];
    if (nameInput.value.trim().length < 2) problems.push('your full name');
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test($('piEmail').value.trim())) problems.push('a valid email');
    const phone = $('piPhone').value.replace(/\D/g, '');
    if (phone.length < 7 || phone.length > 15) problems.push('a valid phone number');
    if (!$('piConsent').checked) problems.push('your agreement to the background check');

    if (problems.length) {
      e.preventDefault();
      err.textContent = 'Please provide ' + problems.join(', ') + '.';
      err.classList.remove('hidden');
      success.classList.add('hidden');
      return;
    }
    err.classList.add('hidden');

    // TODO (backend): remove the next lines to post to ProgrammesController.Interest.
    e.preventDefault();
    success.classList.remove('hidden');
    success.classList.add('flex');
  });
})();
