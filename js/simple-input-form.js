(function () {
    function getValueFromPath(path, obj = window) {
        return path.split('.').reduce((acc, part) => acc?.[part], obj);
    }
    const currentScript = document?.currentScript;
    if (currentScript) {
        const url = new URL(currentScript.src);
        const dynamicFieldPath = url.searchParams.get('dynamicField');
        const isNested = ['1', 'yes', 'true'].includes(url.searchParams.get('nested')?.toLowerCase());;
        if (dynamicFieldPath) {
            const dynamicField = getValueFromPath(dynamicFieldPath);
            initSimpleInputForm(dynamicField, isNested);
        }
    }
})();
function initSimpleInputForm(dynamicField, treatDotInNameAsNestedObject = false) {
    if (!dynamicField) {
        console.error('Missing dynamicFields!');
        return;
    }
    document.addEventListener('DOMContentLoaded', () => {
        const {
            setValue,
            getValue,
            addEventListener,
            querySelectorAll,
        } = dynamicField;

        function parsePath(path) {
            const parts = [];
            const regex = /([^\.\[\]]+)|\[(\d+)\]/g;
            let match;
            while ((match = regex.exec(path)) !== null) {
                if (match[1] !== undefined) {
                    parts.push(match[1]);
                } else if (match[2] !== undefined) {
                    parts.push(Number(match[2]));
                }
            }
            return parts;
        }

        function setNestedValue(obj, path, value) {
            const keys = parsePath(path);
            let current = obj;

            keys.forEach((key, index) => {
                const isLast = index === keys.length - 1;
                const nextKey = keys[index + 1];
                const nextIsArrayIndex = typeof nextKey === 'number';

                if (typeof key === 'number') {
                    if (!Array.isArray(current)) {
                        current = [];
                    }
                    while (current.length <= key) {
                        current.push(null);
                    }
                    if (isLast) {
                        current[key] = value;
                    } else {
                        if (current[key] === null || typeof current[key] !== 'object') {
                            current[key] = nextIsArrayIndex ? [] : {};
                        }
                        current = current[key];
                    }
                } else {
                    if (!(key in current) || typeof current[key] !== 'object' || current[key] === null) {
                        current[key] = nextIsArrayIndex ? [] : {};
                    }
                    if (isLast) {
                        current[key] = value;
                    } else {
                        current = current[key];
                    }
                }
            });
        }

        function getNestedValue(obj, path) {
            const keys = parsePath(path);
            let current = obj;

            for (let key of keys) {
                const isArrayIndex = key !== '' && !isNaN(key);

                if (current == null) return undefined;

                if (isArrayIndex) {
                    current = current[parseInt(key, 10)];
                } else if (key === '') {
                    if (!Array.isArray(current) || current.length === 0) return undefined;
                    current = current[current.length - 1];
                } else {
                    current = current[key];
                }
            }

            return current;
        }

        function flattenObjectToPaths(obj, prefix = '') {
            let result = {};

            if (Array.isArray(obj)) {
                obj.forEach((item, index) => {
                    const path = prefix ? `${prefix}[${index}]` : `[${index}]`;
                    if (typeof item === 'object' && item !== null) {
                        Object.assign(result, flattenObjectToPaths(item, path));
                    } else {
                        result[path] = item;
                    }
                });
            } else if (obj && typeof obj === 'object') {
                for (const key in obj) {
                    if (!obj.hasOwnProperty(key)) continue;
                    const value = obj[key];
                    const path = prefix ? `${prefix}.${key}` : key;

                    if (typeof value === 'object' && value !== null) {
                        Object.assign(result, flattenObjectToPaths(value, path));
                    } else {
                        result[path] = value;
                    }
                }
            } else {
                result[prefix] = obj;
            }

            return result;
        }

        function getDict(allFields, splitDots = false) {
            return Array.from(allFields).reduce((obj, el) => {
                let value;

                if (el.type === 'checkbox') {
                    value = el.checked;
                } else if (el.type === 'radio') {
                    if (el.checked) {
                        value = el.value;
                    } else {
                        const exists = splitDots ? getNestedValue(obj, el.name) : obj.hasOwnProperty(el.name);
                        if (exists) return obj;
                        value = null;
                    }
                } else if (el.tagName === 'SELECT' && el.multiple) {
                    value = Array.from(el.selectedOptions).map(opt => opt.value);
                } else {
                    value = el.value;
                }

                if (splitDots) {
                    setNestedValue(obj, el.name, value);
                } else {
                    obj[el.name] = value;
                }

                return obj;
            }, {});
        }

        function setDict(allFields, dict, splitDots = false) {
            let flatDict = dict;

            if (splitDots) {
                flatDict = flattenObjectToPaths(dict);
            }

            Array.from(allFields).forEach(el => {
                const value = flatDict[el.name];
                if (value === undefined) return;

                if (el.type === 'checkbox') {
                    if (Array.isArray(value)) {
                        el.checked = value.includes(el.value);
                    } else {
                        el.checked = Boolean(value);
                    }
                } else if (el.type === 'radio') {
                    el.checked = el.value === value;
                } else if (el.tagName === 'SELECT' && el.multiple) {
                    Array.from(el.options).forEach(opt => {
                        opt.selected = value.includes(opt.value);
                    });
                } else {
                    el.value = value;
                }
            });
        }

        const buildSelectorForInputTypeEventFields = ({ parentClass = '', fieldClass = '', attribute = '' } = {}) =>
            `${parentClass} input${fieldClass}${attribute}:not([type="checkbox"]):not([type="radio"]):not([type="range"]), textarea${fieldClass}${attribute}`;

        const buildSelectorForChangeTypeEventFields = ({ parentClass = '', fieldClass = '', attribute = '' } = {}) =>
            `${parentClass} select${fieldClass}${attribute}, input${fieldClass}${attribute}[type="checkbox"], input${fieldClass}${attribute}[type="radio"], input${fieldClass}${attribute}[type="range"]`;

        const selectorForInputTypeEventFields = buildSelectorForInputTypeEventFields();
        const selectorForChangeTypeEventFields = buildSelectorForChangeTypeEventFields();

        dynamicField.initializedElements = new WeakSet();
        dynamicField.fields = [];

        const observer = new MutationObserver((mutationsList) => {
            for (let mutation of mutationsList) {
                if (mutation.type === 'childList') {
                    mutation.addedNodes.forEach((node) => {
                        if (node.nodeType === 1) {
                            let matchedElements = [];
                            if (node.matches(selectorForInputTypeEventFields)) {
                                node.inputTypeEvent = true;
                                matchedElements.push(node);
                            } else if (node.matches(selectorForChangeTypeEventFields)) {
                                node.changeTypeEvent = true;
                                matchedElements.push(node);
                            }
                            matchedElements.push(...Array.from(node.querySelectorAll(selectorForInputTypeEventFields), el => (el.inputTypeEvent = true, el)));
                            matchedElements.push(...Array.from(node.querySelectorAll(selectorForChangeTypeEventFields), el => (el.changeTypeEvent = true, el)));
                            if (matchedElements.length > 0) {
                                processElements(matchedElements);
                            }
                        }
                    });
                }
            }
        });

        observer.observe(dynamicField.getElement(), { childList: true, subtree: true, attributes: false });

        function processElements(elements) {
            elements.forEach(element => {
                if (!dynamicField.initializedElements.has(element)) {
                    if (element.inputTypeEvent) {
                        element.addEventListener('input', (event) => {
                            console.log('Input changed:', event.target.value);
                            setValue(getDict(dynamicField.fields, treatDotInNameAsNestedObject));
                        });
                    } else if (element.changeTypeEvent) {
                        element.addEventListener('change', (event) => {
                            console.log('Changed field:', event.target.tagName, type, event.target.value);
                            setValue(getDict(dynamicField.fields, treatDotInNameAsNestedObject));
                        });
                    }
                    dynamicField.initializedElements.add(element);
                    dynamicField.fields.push(element);

                    setDict([element], getValue() ?? {}, treatDotInNameAsNestedObject);
                }
            });
        }

        const inputTypeEventFields = Array.from(querySelectorAll(selectorForInputTypeEventFields), el => (el.inputTypeEvent = true, el));
        const changeTypeEventFields = Array.from(querySelectorAll(selectorForChangeTypeEventFields), el => (el.changeTypeEvent = true, el));

        // init all those already exisitng elements in DOM, all others will be initialized by mutation observer
        processElements([...inputTypeEventFields, ...changeTypeEventFields]);

        addEventListener('value', (object) => {
            setDict(dynamicField.fields, object ?? {}, treatDotInNameAsNestedObject);
        }, { init: true });
    });
}
