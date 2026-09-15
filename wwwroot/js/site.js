//crear la funcion activar sala, que cree

const HollowDialog = (() => {
    let dialog = null;
    let titleEl = null;
    let textEl = null;
    let choicesEl = null;
    let avatarEl = null;

    function ensureElements() {
        if (!dialog) {
            dialog = document.getElementById('hollow-dialog');
            if (!dialog) return false;

            titleEl = document.getElementById('hollow-dialog-title');
            textEl = document.getElementById('hollow-dialog-text');
            choicesEl = document.getElementById('hollow-dialog-choices');
            avatarEl = document.getElementById('hollow-dialog-avatar');

            dialog.addEventListener('click', (event) => {
                if (event.target === dialog || event.target.classList.contains('hollow-dialog__backdrop')) {
                    closeDialog();
                }
            });
        }

        return true;
    }

    function closeDialog() {
        if (!ensureElements()) return;

        dialog.classList.add('hidden');
        dialog.setAttribute('aria-hidden', 'true');
        textEl.textContent = '';
        titleEl.textContent = 'Hornet';
        avatarEl.textContent = 'N';
        choicesEl.innerHTML = '';
    }

    function renderChoices(options = []) {
        if (!ensureElements()) return;

        choicesEl.innerHTML = '';

        options.forEach((option) => {
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'hollow-dialog__choice';
            button.textContent = option.label;
            button.addEventListener('click', () => {
                closeDialog();
                if (typeof option.action === 'function') {
                    option.action();
                }
            });
            choicesEl.appendChild(button);
        });
    }

    function typeText(message, speed = 20) {
        if (!ensureElements()) return;

        textEl.textContent = '';

        let index = 0;
        const interval = setInterval(() => {
            textEl.textContent += message[index] ?? '';
            index++;

            if (index >= message.length) {
                clearInterval(interval);
            }
        }, speed);
    }

    function show({
        speaker = 'Hornet',
        avatar = speaker.charAt(0).toUpperCase(),
        message = '',
        choices = []
    }) {
        dialog.classList.remove('hidden');
        dialog.setAttribute('aria-hidden', 'false');
        titleEl.textContent = speaker.toUpperCase();
        avatarEl.textContent = avatar.toUpperCase().slice(0, 1);

        const texto = Array.isArray(message) ? message.join(' ') : message;
        typeText(texto);
        renderChoices(choices);
    }

    function open({
        speaker = 'Hornet',
        avatar = speaker.charAt(0).toUpperCase(),
        message = '',
        choices = []
    }) {
        return new Promise((resolve) => {
            const buttons = choices.length > 0 ? choices : [{
                label: 'Continuar',
                action: () => resolve(true)
            }];

            show({
                speaker,
                avatar,
                message,
                choices: buttons.map((option) => ({
                    ...option,
                    action: () => {
                        if (typeof option.action === 'function') {
                            option.action();
                        }
                        resolve(option.value ?? true);
                    }
                }))
            });
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        ensureElements();
    });

    return { show, open, close: closeDialog };
})();

window.HollowDialog = HollowDialog;