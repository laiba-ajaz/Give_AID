// Mobile navbar toggle (shown below the xl breakpoint).
(function () {
  const toggle = document.getElementById('navToggle');
  const panel = document.getElementById('mobileNav');
  if (!toggle || !panel) return;

  toggle.addEventListener('click', function () {
    const open = panel.classList.toggle('hidden') === false;
    toggle.setAttribute('aria-expanded', String(open));
    toggle.querySelector('.material-symbols-outlined').textContent = open ? 'close' : 'menu';
  });

  // Close the panel after choosing a link (useful for #about / #partners anchors).
  panel.addEventListener('click', function (e) {
    if (e.target.closest('a')) {
      panel.classList.add('hidden');
      toggle.setAttribute('aria-expanded', 'false');
      toggle.querySelector('.material-symbols-outlined').textContent = 'menu';
    }
  });
})();
