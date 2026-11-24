// Give-AID live chat box (Help Centre).
// Visitor ki chat ka token browser (localStorage) mein rehta hai, taake page refresh par bhi wahi chat khule.
(function () {
    const box = document.getElementById('chatBox');
    if (!box) return;

    const TOKEN_KEY = 'giveaid.chatToken';
    const POLL_MS = 4000;

    const urls = {
        status: box.dataset.statusUrl,
        start: box.dataset.startUrl,
        send: box.dataset.sendUrl,
        messages: box.dataset.messagesUrl
    };
    const antiForgery = box.querySelector('input[name="__RequestVerificationToken"]').value;

    const startForm = document.getElementById('chatStart');
    const conversation = document.getElementById('chatConversation');
    const list = document.getElementById('chatMessages');
    const sendForm = document.getElementById('chatSend');
    const textBox = document.getElementById('chatText');
    const closedNote = document.getElementById('chatClosed');
    const errorText = box.querySelector('[data-chat-error]');

    let lastId = 0;
    let pollTimer = null;

    // localStorage private mode mein fail ho sakta hai
    function getToken() { try { return localStorage.getItem(TOKEN_KEY); } catch { return null; } }
    function setToken(t) { try { t ? localStorage.setItem(TOKEN_KEY, t) : localStorage.removeItem(TOKEN_KEY); } catch { } }

    function show(el, on, display) {
        el.classList.toggle('hidden', !on);
        if (display) el.classList.toggle(display, on);
    }

    function post(url, data) {
        const body = new URLSearchParams(data);
        body.append('__RequestVerificationToken', antiForgery);
        return fetch(url, { method: 'POST', body: body }).then(function (res) {
            return res.json().catch(function () { return {}; }).then(function (json) {
                if (!res.ok) throw new Error(json.error || 'Something went wrong. Please try again.');
                return json;
            });
        });
    }

    // ---------- Online / offline ----------

    function setStatus(online) {
        document.querySelectorAll('[data-chat-dot]').forEach(function (dot) {
            dot.classList.toggle('bg-emerald-500', online);
            dot.classList.toggle('bg-outline', !online);
        });
        document.querySelectorAll('[data-chat-ping]').forEach(function (p) { p.classList.toggle('hidden', !online); });
        document.querySelectorAll('[data-chat-status]').forEach(function (el) {
            el.textContent = online ? 'Online – we usually reply in a few minutes' : 'Offline – leave a message';
        });
        document.querySelectorAll('[data-chat-banner]').forEach(function (el) {
            el.textContent = online ? 'Live Chat Status: Online' : 'Live Chat Status: Offline';
        });
        document.querySelectorAll('[data-chat-sub]').forEach(function (el) {
            el.textContent = online
                ? 'A member of our team is available right now.'
                : 'No one is online right now. Leave a message and we will reply by email.';
        });
    }

    function checkStatus() {
        fetch(urls.status).then(function (r) { return r.json(); }).then(function (s) { setStatus(s.online); }).catch(function () { });
    }

    // ---------- Messages ----------

    function addMessage(m) {
        if (m.id <= lastId) return;
        lastId = m.id;

        const row = document.createElement('div');
        row.className = 'flex flex-col max-w-[85%] ' + (m.fromAdmin ? 'self-start items-start' : 'self-end items-end');

        const bubble = document.createElement('div');
        bubble.className = 'px-space-md py-space-xs rounded-xl font-body-sm text-body-sm whitespace-pre-line break-words ' +
            (m.fromAdmin ? 'bg-surface-container-lowest text-on-surface shadow-sm' : 'bg-primary text-on-primary');
        bubble.textContent = m.text;   // textContent: HTML kabhi run nahi hota

        const meta = document.createElement('span');
        meta.className = 'mt-1 font-label-sm text-label-sm text-outline';
        meta.textContent = (m.fromAdmin ? 'Give-AID · ' : 'You · ') + m.time;

        row.appendChild(bubble);
        row.appendChild(meta);
        list.appendChild(row);
        list.scrollTop = list.scrollHeight;
    }

    function addNotice(text) {
        const p = document.createElement('p');
        p.className = 'self-center text-center font-label-sm text-label-sm text-outline px-space-md';
        p.textContent = text;
        list.appendChild(p);
    }

    function loadMessages() {
        const token = getToken();
        if (!token) return;

        fetch(urls.messages + '?token=' + encodeURIComponent(token) + '&after=' + lastId)
            .then(function (res) {
                if (res.status === 404) { resetChat(); return null; }
                return res.json();
            })
            .then(function (data) {
                if (!data) return;
                data.messages.forEach(addMessage);
                setStatus(data.online);
                show(closedNote, data.closed);
                show(sendForm, !data.closed, 'flex');
                if (data.closed) stopPolling();
            })
            .catch(function () { });
    }

    function startPolling() {
        stopPolling();
        loadMessages();
        pollTimer = setInterval(loadMessages, POLL_MS);
    }

    function stopPolling() {
        if (pollTimer) clearInterval(pollTimer);
        pollTimer = null;
    }

    // ---------- Screens ----------

    function showConversation() {
        show(startForm, false, 'flex');
        show(conversation, true, 'flex');
        if (list.childElementCount === 0) {
            addNotice('Chat started. Send us your question and a team member will reply here.');
        }
        startPolling();
        textBox.focus();
    }

    function resetChat() {
        stopPolling();
        setToken(null);
        lastId = 0;
        list.innerHTML = '';
        show(closedNote, false);
        show(sendForm, true, 'flex');
        show(conversation, false, 'flex');
        show(startForm, true, 'flex');
    }

    function openChat() {
        show(box, true, 'flex');
        checkStatus();
        if (getToken()) showConversation();
        else show(startForm, true, 'flex');
    }

    function closeChat() {
        show(box, false, 'flex');
        stopPolling();
    }

    // ---------- Events ----------

    startForm.addEventListener('submit', function (e) {
        e.preventDefault();
        show(errorText, false);
        if (!startForm.checkValidity()) { startForm.reportValidity(); return; }

        post(urls.start, { name: startForm.elements['name'].value, email: startForm.elements['email'].value })
            .then(function (res) { setToken(res.token); showConversation(); })
            .catch(function (err) { errorText.textContent = err.message; show(errorText, true); });
    });

    sendForm.addEventListener('submit', function (e) {
        e.preventDefault();
        const text = textBox.value.trim();
        if (!text) return;

        textBox.value = '';
        post(urls.send, { token: getToken(), text: text })
            .then(addMessage)
            .catch(function (err) { textBox.value = text; addNotice(err.message); });
    });

    // Enter = send, Shift+Enter = nayi line
    textBox.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendForm.requestSubmit();
        }
    });

    document.querySelectorAll('[data-open-chat]').forEach(function (btn) { btn.addEventListener('click', openChat); });
    box.querySelector('[data-close-chat]').addEventListener('click', closeChat);
    box.querySelector('[data-new-chat]').addEventListener('click', resetChat);
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeChat(); });

    checkStatus();
    setInterval(checkStatus, 30000);
})();
