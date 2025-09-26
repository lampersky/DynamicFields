if (!window.ImageUploadComponent) {
    window.ImageUploadComponent = class ImageUploadComponent extends HTMLElement {
        constructor() {
            super();
            this.attachShadow({ mode: 'open' });
            this.shadowRoot.innerHTML = `
              <div style="display: flex; margin-bottom: 1rem;">
                <input type="file" accept="image/*">
                <button>crop</button>
              </div>
              <slot></slot>
            `;
        }

        connectedCallback() {
            const { addEventListener, getValue, setValue, getPathBase, getElement } = window.dynamicFields?.[this.getAttribute('dynamic-field-parent-id')];
            this.setValue = setValue;
            this.pathBase = getPathBase();

            this.fileInput = this.shadowRoot.querySelector('input[type="file"]');
            this.shadowRoot.querySelector('button').addEventListener('click', async () => {
                const { blob, dataURL } = await this.getCroppedBlob();
                this.fileInput.value = null;
                const name = `${this.uuidv4()}.png`;
                await this.upload(blob, name)
                    .then(response => {
                        this.cropElement.src = dataURL;
                        this.setValue(response.result);
                    })
                    .catch(error => {
                        console.error('Can\'t upload:', error);
                    });
                const iframe = window.self !== window.top;
                if (!iframe) {
                    window.addEventListener("beforeunload", this.beforeUnloadHandler);
                }
            });

            const slot = this.shadowRoot.querySelector('slot');
            this.fileInput.addEventListener('change', (event) => {
                this.handleFile(event);
            });

            slot.addEventListener('slotchange', () => {
                this.cropElement = slot.assignedElements().find(el => el.tagName.toLowerCase() === 'image-crop');
                if (this.cropElement) {
                    customElements.whenDefined('image-crop').then(() => {
                        this.cropElement.addEventListener('image-crop-change', (event) => {
                            const { x, y, width, height } = event.detail;
                            this.lastCrop = { x, y, width, height };
                        });
                        addEventListener('value', (value) => {
                            if (this.cropElement && value?.mediaPath) {
                                this.cropElement.src = '/media/' + value.mediaPath;
                            }
                        }, { init: true });
                    });
                }
            });

            const form = getElement().closest('form');
            if (form) {
                form.querySelectorAll('button[type="submit"]').forEach(button => {
                    button.addEventListener('click', () => {
                        window.removeEventListener("beforeunload", this.beforeUnloadHandler)
                    });
                });
            }
        }

        uuidv4() {
            // https://caniuse.com/getrandomvalues
            return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c => (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16));
        }

        beforeUnloadHandler(event) {
            event.preventDefault();
            event.returnValue = "";
        }

        handleFile(event) {
            const file = event.target.files[0];
            if (file && this.cropElement) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    this.cropElement.src = e.target.result;
                };
                reader.readAsDataURL(file);
            }
        }

        emitCropData(cropData) {
            const { x, y, width, height } = cropData;
            this.lastCrop = { x, y, width, height };
        }

        async getCroppedBlob(type = 'image/png', quality = 0.92) {
            if (!this.cropElement || !this.cropElement.src) return null;

            const img = new Image();
            img.src = this.cropElement.src;

            await new Promise((resolve) => {
                img.onload = resolve;
            });

            const { x = 0, y = 0, width = img.width, height = img.height } = this.lastCrop || {};

            const canvas = document.createElement('canvas');
            canvas.width = width;
            canvas.height = height;
            const ctx = canvas.getContext('2d');

            ctx.drawImage(img, x, y, width, height, 0, 0, width, height);

            return new Promise((resolve) => {
                canvas.toBlob((blob) => {
                    const dataURL = canvas.toDataURL();
                    resolve({ blob, dataURL });
                }, type, quality);
            });
        }

        async upload(blob, name) {
            const token = document.querySelector("input[name=__RequestVerificationToken]").value;
            const formData = new FormData();
            const dt = new DataTransfer();
            const file = new File([blob], name);
            dt.items.add(file);
            formData.append('path', 'dynamicFields');
            formData.append('contentType', 'image/png');
            formData.append('__RequestVerificationToken', token);
            formData.append('files', dt.files[0]);

            return fetch(`${this.pathBase}/Admin/Media/Upload`, {
                method: 'POST',
                body: formData
            })
            .then(response => {
                if (response.ok) {
                    return response.json();
                }
                throw `Server responded with code: ${response.status}.`;
            })
            .then(result => {
                if (result && result.files && result.files[0]) {
                    const firstFileResult = result.files[0];
                    const uploadResult = { success: !firstFileResult.error, result: firstFileResult, error: firstFileResult.error };
                    return uploadResult;
                }
            })
            .catch(error => {
                const uploadResult = { success: false, error: error };
                return uploadResult;
            });
        }
    }
}
if (!window.customElements.get('image-upload-component')) {
    window.customElements.define('image-upload-component', window.ImageUploadComponent);
}