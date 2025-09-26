if (!window.SimpleImageGallery) {
    window.SimpleImageGallery = class SimpleImageGallery extends HTMLElement {
        constructor() {
            super();
        }
        async connectedCallback() {
            this.parentId = this.getAttribute('dynamic-field-parent-id');

            const images = [
                'https://fastly.picsum.photos/id/223/200/200.jpg?hmac=CNNyWbBcEAJ7TPkTmEEwdGrLFEYkxpTeVwJ7U0LB30Y',
                'https://fastly.picsum.photos/id/373/200/200.jpg?hmac=WAwyn7yIFXuyUxxF4b3ijw7qJfIP7oBXicnozVoLj_o',
                'https://fastly.picsum.photos/id/523/200/200.jpg?hmac=d1qFeOBBhPqpCZ0U-197Ibo1qK82CmzUfDfKVS70O24',
                'https://fastly.picsum.photos/id/665/200/200.jpg?hmac=hWcfvzYgHAwJFOUaHZa2oZpOOL7yx_x8Bnhq0dFVQRw',
                'https://fastly.picsum.photos/id/85/200/200.jpg?hmac=gQRdT-HPw1azaNf38WLW_QZv7aC0WjwOSM4Sf5kWm3U',
                'https://fastly.picsum.photos/id/25/200/200.jpg?hmac=G4ZRBi0qdWfQJQs_yxNQr_LJJlf0V1_Pdj8Tp41xsJU',
                'https://fastly.picsum.photos/id/870/200/200.jpg?hmac=G4IaFUfMAbn5JlMY8wZINYyI9gol9fXYZXdaVEF5Jzg',
                'https://fastly.picsum.photos/id/368/200/200.jpg?hmac=ej5Lmr5qh7f88zx85PnlHj2GKfwrNNWf6-lACRJ34qI',
                'https://fastly.picsum.photos/id/957/200/200.jpg?hmac=EEtYnA2sFaZhrEO8XM9AI5QNDaACfYTyRwgdMNrQeDQ',
            ];

            const imagePlaceholder = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIyMDAiIGhlaWdodD0iMjAwIiB2aWV3Qm94PSIwIDAgMjAwIDIwMCI+CiAgPHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgZmlsbD0iI2NjY2NjYyIvPgogIDx0ZXh0IHg9IjUwJSIgeT0iNTAlIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkb21pbmFudC1iYXNlbGluZT0ibWlkZGxlIgogICAgICAgZm9udC1mYW1pbHk9IkFyaWFsLCBIZWx2ZXRpY2EsIHNhbnMtc2VyaWYiIGZvbnQtc2l6ZT0iMjAiIGZpbGw9IiMyMjIyMjIiPgogICAgY2xpY2sgaGVyZQogIDwvdGV4dD4KPC9zdmc+";

            this.innerHTML = `
            <div class="position-relative d-inline-block" data-bs-toggle="modal" data-bs-target="#${this.parentId}_imageModal">
              <div class="d-flex justify-content-center">
                  Click here to open gallery
              </div>
                <img src="${imagePlaceholder}" class="selected-img img-fluid rounded"
                   onload="this.previousElementSibling.classList.add('d-none'); this.classList.remove('d-none');">
            </div>

            <div class="modal fade" id="${this.parentId}_imageModal" tabindex="-1" aria-labelledby="${this.parentId}_imageModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="${this.parentId}_imageModalLabel">Choose Image</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="container">
                    <div class="gallery-items row">
                        ${                   
                            images.map(url => `
                              <div class="col-4 p-2">
                                  <img src="${url}" class="gallery-img img-fluid rounded" data-bs-dismiss="modal" />
                              </div>
                            `).join('')
                        }
                    </div>
                    </div>
                </div>
                </div>
            </div>
            </div>
        `;
            const saveState = (src) => {
                this.onChange({ src });
            };
            this.querySelectorAll('.gallery-img').forEach(img => {
                img.addEventListener('click', (e) => saveState(e.target.src));
            });
        }
        setImage(imageUrl) {
            const selectedImageEl = this.querySelector('.selected-img');
            selectedImageEl.previousElementSibling.classList.remove('d-none');
            selectedImageEl.classList.add('d-none');
            selectedImageEl.src = `${imageUrl}`;
        }
        onChange(image) {
            // leave it empty
        }
    }
}
if (!window.customElements.get('simple-image-gallery')) {
    window.customElements.define('simple-image-gallery', window.SimpleImageGallery);
}