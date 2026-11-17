// Shows a preview of the image the admin picks in a file input.
// Usage: <input type="file" data-preview="imgElementId" />
document.addEventListener('change', function (e) {
    const input = e.target;
    if (!(input instanceof HTMLInputElement) || input.type !== 'file' || !input.dataset.preview) return;

    const img = document.getElementById(input.dataset.preview);
    if (!img) return;

    const file = input.files && input.files[0];
    if (file) {
        if (file.size > 2 * 1024 * 1024) {
            alert('Image must be smaller than 2 MB.');
            input.value = '';
            return;
        }
        img.src = URL.createObjectURL(file);
        img.hidden = false;
    } else if (img.dataset.original) {
        img.src = img.dataset.original;   // edit modal: back to the current image
    } else {
        img.removeAttribute('src');
        img.hidden = true;
    }
});

// Edit modal helper: show the record's current image (or nothing) and reset the file input.
function setCurrentImage(imgId, inputId, path) {
    const img = document.getElementById(imgId);
    const input = document.getElementById(inputId);
    input.value = '';
    img.dataset.original = path || '';
    if (path) { img.src = path; img.hidden = false; }
    else { img.removeAttribute('src'); img.hidden = true; }
}
