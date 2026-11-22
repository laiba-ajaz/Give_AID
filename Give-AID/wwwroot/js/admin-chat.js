// Admin Live Chat page: reply bhejna aur har 4 second baad naye messages / chat list update karna.
// Page khula rehna hi admin ko website par "Online" dikhata hai (har poll server ko batata hai).
(function () {
    const POLL_MS = 4000;

    const box = document.getElementById('adminMessages');
    const list = document.getElementById('chatList');
    const form = document.getElementById('adminReply');

    // Koi chat select nahi (ya koi chat hi nahi): phir bhi online dikhne ke liye page reload karte raho
    if (!box) {
        setInterval(function () { location.reload(); }, 15000);
        return;
    }

    const chatId = box.dataset.id;
    const visitorName = (list.querySelector('.chat-item.active b') || {}).textContent || 'Visitor';
    let lastId = parseInt(box.dataset.last, 10) || 0;

    function scrollDown() { box.scrollTop = box.scrollHeight; }

    function addMessage(m) {
        if (m.id <= lastId) return;
        lastId = m.id;

        const empty = document.getElementById('noMessages');
        if (empty) empty.remove();

        const row = document.createElement('div');
        row.className = 'bubble-row' + (m.fromAdmin ? ' me' : '');

        const bubble = document.createElement('div');
        bubble.className = 'bubble';
        bubble.textContent = m.text;

        const meta = document.createElement('span');
        meta.className = 'bubble-meta';
        meta.textContent = (m.fromAdmin ? 'You' : visitorName) + ' · ' + m.time;

        row.appendChild(bubble);
        row.appendChild(meta);
        box.appendChild(row);
        scrollDown();
    }

    // Bayein list ki ginti aur aakhri message update karo (nayi chat aaye to page reload)
    function updateList(sessions) {
        const known = list.querySelectorAll('.chat-item').length;
        if (sessions.length !== known) { location.reload(); return; }

        sessions.forEach(function (s) {
            const item = list.querySelector('.chat-item[data-id="' + s.id + '"]');
            if (!item) return;

            item.querySelector('.chat-item-text').textContent = (s.closed ? 'Closed · ' : '') + (s.lastText || 'No messages yet');
            item.querySelector('small').textContent = s.lastTime;

            let badge = item.querySelector('.nav-count');
            if (s.unread > 0) {
                if (!badge) {
                    badge = document.createElement('span');
                    badge.className = 'nav-count';
                    item.appendChild(badge);
                }
                badge.textContent = s.unread;
            } else if (badge) {
                badge.remove();
            }
        });
    }

    function poll() {
        fetch(box.dataset.pollUrl + '?id=' + chatId + '&after=' + lastId)
            .then(function (r) { return r.json(); })
            .then(function (data) {
                data.messages.forEach(addMessage);
                updateList(data.sessions);
            })
            .catch(function () { });
    }

    if (form) {
        const text = document.getElementById('replyText');
        const token = form.querySelector('input[name="__RequestVerificationToken"]').value;

        form.addEventListener('submit', function (e) {
            e.preventDefault();
            const value = text.value.trim();
            if (!value) return;

            const body = new URLSearchParams({ id: chatId, text: value, __RequestVerificationToken: token });
            text.value = '';

            fetch(box.dataset.replyUrl, { method: 'POST', body: body })
                .then(function (r) { return r.json().then(function (j) { if (!r.ok) throw new Error(j.error || 'Could not send.'); return j; }); })
                .then(addMessage)
                .catch(function (err) { text.value = value; alert(err.message); });
        });

        text.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                form.requestSubmit();
            }
        });
        text.focus();
    }

    scrollDown();
    setInterval(poll, POLL_MS);
})();
