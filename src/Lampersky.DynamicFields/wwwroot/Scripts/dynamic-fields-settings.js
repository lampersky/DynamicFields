function clearQueryParams() {
    const url = new URL(window.location);
    ['contentType', 'contentField', 'url'].forEach(p => url.searchParams.delete(p));
    history.replaceState(null, '', url);
}

function toggleEditor(input) {
    input.closest('.resource').querySelector('textarea').classList.toggle('single-line-textarea');
}

function preventEnter(textarea, event) {
    if (textarea.classList.contains('single-line-textarea') && event.key === 'Enter') {
        event.preventDefault();
    }
}

function once(element, event, handler) {
    const wrappedHandler = function (...args) {
        element.removeEventListener(event, wrappedHandler);
        handler.apply(null, args);
    };
    element.addEventListener(event, wrappedHandler);
}

function debounce(func, delay) {
    let timeoutId;
    return function (...args) {
        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => {
            func.call(this, ...args);
        }, delay);
    };
}

function openExternalEditor(input, event) {
    event.preventDefault();
    const resource = [...input.parentElement.querySelectorAll('input')].reduce((acc, x) => {
        const key = x.id.match(/_([^_]*)$/)?.[1];
        if (key) acc[key] = x.checked;
        return acc;
    }, {});

    if (resource?.IsInline === false) {
        event.preventDefault();
        return;
    }
    
    const editor = CodeMirrorModule.getEditor();
    const wrapper = editor.getWrapperElement();
    const currentMode = editor.getOption("mode")?.name;
    wrapper.style.opacity = "0";
    wrapper.style.transition = "opacity 0.5s ease";

    editor.operation(() => {
        editor.setValue(input.value);
        if (resource?.IsStyle) {
            editor.setOption("mode", "css");
        } else if (resource?.IsScript) {
            editor.setOption("mode", "javascript");
        } else if (currentMode != "htmlmixed") {
            editor.setOption("mode", "htmlmixed");
        }
    });

    const codeEditorModalElement = document.getElementById("codeEditorModal");
    let codeEditorModal = bootstrap.Modal.getInstance(codeEditorModalElement);

    if (!codeEditorModal) {
        codeEditorModal = new bootstrap.Modal(codeEditorModalElement);
    }

    once(codeEditorModalElement, "shown.bs.modal", () => {
        editor.refresh();
        wrapper.style.opacity = "1";
    });

    once(codeEditorModalElement, "hidden.bs.modal", () => {
        input.value = editor.getValue();
    }, { once: true });

    codeEditorModal.show();
}

function onResourceTypeChange(input) {
    input.closest('.dropdown-menu').querySelectorAll('.only-for-script').forEach(element => {
        if (input.value == "Script") {
            element.classList.remove('disabled');
        } else {
            element.classList.add('disabled');
        }
    });
}

function getLastIndex() {
    return Array.from(document.querySelectorAll('div.resources > .resource input')).reduce((max, input) => {
        const matches = input.id?.match(/(?:_(\d+)_)/);
        if (matches) {
            const found = parseInt(matches[1], 10);
            return Math.max(max, found);
        }
        return max;
    }, 0);
}

function reindex(resource, index) {
    const elements = resource.querySelectorAll('input, select, textarea, label');
    elements.forEach((element) => {
        if (element.hasAttribute('for')) {
            const newFor = element.getAttribute('for').replace(/(?:_(\d+)_)/, (match, offset) => `_${index}_`);
            element.setAttribute('for', newFor);
        } else {
            element.name = element.name.replace(/(?:\[(\d+)\])/, (match, offset) => `[${index}]`);
            element.id = element.id.replace(/(?:_(\d+)_)/, (match, offset) => `_${index}_`);
        }
    });
}

function reindexAll() {
    const resourcesContainer = document.querySelector('div.resources');
    const resources = document.querySelectorAll('div.resources > .resource');
    resources.forEach((resource, index) => {
        // detach element, to avoid trigerring radio button states, when there is name collision
        resource.remove();
        // change name, id, for
        reindex(resource, index);
    });
    resources.forEach((resource) => {
        // attach elements back
        resourcesContainer.appendChild(resource);
    });
}

function move(button, dir) {
    const resourceToMove = button.closest('.resource');
    const itemsContainer = document.querySelector('div.resources');
    const children = Array.from(itemsContainer.children);

    const idx = children.indexOf(resourceToMove);

    if (dir == -1 && idx > 0) {
        itemsContainer.insertBefore(resourceToMove, children[idx + dir]);
    } else if (dir == 1 && idx < children.length - 1) {
        itemsContainer.insertBefore(children[idx + dir], resourceToMove);
    }

    reindexAll();
}

function removeResource(button) {
    const resourceToRemove = button.closest('.resource');
    const resources = resourceToRemove.parentElement;
    resources.removeChild(resourceToRemove);

    reindexAll();
}

function addResource() {
    const template = document.getElementById('resource-template');
    const cloned = template.content.cloneNode(true);
    const itemsContainer = document.querySelector('div.resources');

    reindex(cloned, getLastIndex() + 1);

    itemsContainer.appendChild(cloned);

    reindexAll();
}

function initFakeConsole() {
    const iframe = document.querySelector('iframe[name="previewFrame"]');
    const fakeConsole = document.querySelector('.fake-console');

    const log = (group, msg) => {
        const line = document.createElement("div");
        line.classList.add(group);
        const time = (new Date()).toJSON().split("T")[1].split("Z")[0];
        line.textContent = `[${time}] ${msg}`;
        fakeConsole.appendChild(line);
        fakeConsole.scrollTop = fakeConsole.scrollHeight;
    };

    fakeConsole.log = function (group, msg) {
        log(group, msg);
    };

    window.addEventListener("message", (e) => {
        if (e.source === iframe.contentWindow) {
            const { error } = e.data;
            log('critical', `${error}`);
        }
    });
}

function initFormChangeListener() {
    const form = document.querySelector('form:has(.resources)');
    const refreshButton = document.querySelector('input[type=submit]');
    const debouncedRefresh = debounce(() => {
        const formTarget = form.target;
        const formAction = form.action;
        form.target = refreshButton.formTarget;
        form.action = refreshButton.formAction;
        form.method = 'POST';
        form.submit();
        form.target = formTarget;
        form.action = formAction;
    }, 500);
    form.addEventListener('input', (event) => {
        if (event.target.matches('input[type="text"], textarea')) {
            debouncedRefresh();
        }
    });
    form.addEventListener('change', (event) => {
        if (['checkbox', 'radio', 'select'].includes(event.target.type)) {
            debouncedRefresh();
        }
    });

    return {
        debouncedRefresh
    };
}

function clearFakeConsole() {
    const fakeConsole = document.querySelector('.fake-console');
    while (fakeConsole.firstChild) {
        fakeConsole.removeChild(fakeConsole.firstChild);
    }
}

function saveState(fieldId) {
    const fakeConsole = document.querySelector('.fake-console');
    const iframe = document.querySelector('iframe[name="previewFrame"]');
    const iframeWindow = iframe.contentWindow;
    const { getValue } = iframeWindow.dynamicFields?.[fieldId] ?? {};
    if (getValue && localStorage) {
        try {
            const object = getValue();
            localStorage.setItem(fieldId, JSON.stringify(object));
            fakeConsole.log('info', 'State saved!');
        } catch (e) {
            console.error('Save state failed! Something went wrong, during object serialization!', e);
        }
    }
}

function restoreState(fieldId) {
    const fakeConsole = document.querySelector('.fake-console');
    const iframe = document.querySelector('iframe[name="previewFrame"]');
    const iframeWindow = iframe.contentWindow;
    const { setValue, notify } = iframeWindow.dynamicFields?.[fieldId] ?? {};
    if (setValue && localStorage) {
        try {
            var object = JSON.parse(localStorage.getItem(fieldId));
            setValue(object);
            notify();
            fakeConsole.log('info', 'State restored!');
        } catch (e) {
            console.error('Restore state failed! Something went wrong, during object serialization!', e);
        }
    }
}

async function handleRepositoryChange(select) {
    const container = select.nextElementSibling;
    container.innerHTML = '';
    
    try {
        const response = await fetch(select.value);
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        const data = await response.json();
        data.forEach(item => {
            const a = document.createElement('a');
            a.href = `?url=${encodeURIComponent(item.url)}`;
            a.textContent = `${item.name}@${item.version}`;
            container.appendChild(a);
        });
    } catch (error) {
        console.error(`Something went wrong '${select.value}'`, error);
        const errorMessage = container.getAttribute('data-fetch-failed');
        const div = document.createElement('div');
        div.innerHTML = `<span class="badge text-bg-danger">${errorMessage}</span>`;
        container.appendChild(div);
    }
} 

const CodeMirrorModule = (() => {
    let editor = null;
    let textarea = null;

    function init(textareaId) {
        textarea = document.getElementById(textareaId);
        if (!textarea) {
            console.error(`Textarea with id "${textareaId}" not found.`);
            return;
        }

        editor = CodeMirror.fromTextArea(textarea, {
            lineNumbers: true,
            styleActiveLine: true,
            mode: { name: "javascript" },
        });
    }

    function getEditor() {
        return editor;
    }

    function setValue(value) {
        textarea.value;
    }

    function getValue() {
        return textarea.value;
    }

    return {
        init,
        getEditor,
        setValue,
        getValue
    };
})();

document.addEventListener('DOMContentLoaded', () => {
    clearQueryParams();

    CodeMirrorModule.init('codeMirrorEditor');
    initFakeConsole();
    const { debouncedRefresh } = initFormChangeListener();
    debouncedRefresh();
});