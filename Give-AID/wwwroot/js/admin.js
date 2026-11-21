// Give-AID admin: shared behaviour for every admin page.

// Success messages fade out on their own; errors stay until dismissed.
document.querySelectorAll('.alert-success').forEach(function (el) {
    setTimeout(function () {
        el.classList.add('hide');
        setTimeout(function () { el.remove(); }, 400);
    }, 4000);
});

// Clicking the dark area around a modal closes it.
document.querySelectorAll('dialog.modal').forEach(function (dlg) {
    dlg.addEventListener('click', function (e) {
        if (e.target === dlg) dlg.close();
    });
});

// Opening an "Add" modal starts from a clean form.
function openAddModal(id) {
    const dlg = document.getElementById(id);
    const form = dlg.querySelector('form');
    form.reset();
    dlg.querySelectorAll('.uploader-preview img').forEach(function (img) {
        img.removeAttribute('src');
        img.hidden = true;
        img.dataset.original = '';
    });
    updateCharCounts(dlg);
    dlg.showModal();
}

// Live "n / max" counter under textareas that have data-count.
function updateCharCounts(scope) {
    (scope || document).querySelectorAll('textarea[data-count]').forEach(function (ta) {
        const out = document.getElementById(ta.dataset.count);
        if (out) out.textContent = ta.value.length + ' / ' + ta.maxLength;
    });
}
document.addEventListener('input', function (e) {
    if (e.target.matches && e.target.matches('textarea[data-count]')) updateCharCounts(e.target.parentElement);
});

// Ask before deleting. Usage: <form data-confirm="Delete X?">
document.addEventListener('submit', function (e) {
    const msg = e.target.dataset && e.target.dataset.confirm;
    if (msg && !confirm(msg)) e.preventDefault();
});

// Escape closes the mobile sidebar.
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') document.body.classList.remove('nav-open');
});
