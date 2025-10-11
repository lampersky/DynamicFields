if (window != window.parent) {
    (function () {
        function sendToParent(message) {
            if (window.parent) {
                window.parent.postMessage({ error: message }, "*");
            }
        }

        window.addEventListener('error', function (e) {
            var msg = "";
            if (e.target && (e.target.src || e.target.href)) {
                msg = `Resource failed to load: ${e.target.src || e.target.href}`;
            } else {
                msg = `JS Error: ${e.message} at ${e.filename}:${e.lineno}:${e.colno}`;
            }
            sendToParent(msg);
            e.preventDefault();
        }, true);

        window.addEventListener("unhandledrejection", (e) => {
            sendToParent(`Unhandled Promise Rejection: ${e.reason}`);
        });

        window.onerror = function (message, source, lineno, colno, error) {
            sendToParent(`Inline Script Error: ${message} at ${source}:${lineno}:${colno}`);
            return false;
        };

        ["log", "warn", "error"].forEach(method => {
            const original = window.console[method];
            window.console[method] = function (...args) {
                const msg = args.map(a =>
                    (typeof a === "object" ? JSON.stringify(a) : a)
                ).join(" ");
                log(method, `${msg}`);
                original.apply(window.console, args);
            };
        });
    })();
}