if (!window.GeoCoordinatesComponent) {
    window.GeoCoordinatesComponent = class GeoCoordinatesComponent extends HTMLElement {
        constructor() {
            super();
            /*this.attachShadow({ mode: 'open' });*/
        }

        connectedCallback() {
            this.parentId = this.getAttribute('dynamic-field-parent-id');
            this./*shadowRoot.*/innerHTML = `
            <div class="card mt-3">
              <div class="card-body">
                <div class="mb-3">
                  <label for="${this.parentId}_latitude" class="form-label">Latitude:</label>
                  <input type="text" class="form-control" id="${this.parentId}_latitude" placeholder="Enter latitude">
                </div>
                <div class="mb-3">
                  <label for="${this.parentId}_longitude" class="form-label">Longitude:</label>
                  <input type="text" class="form-control" id="${this.parentId}_longitude" placeholder="Enter longitude">
                </div>
              </div>
            </div>
        `;

            this.latitude = this./*shadowRoot.*/querySelector(`input#${this.parentId}_latitude`);
            this.longitude = this./*shadowRoot.*/querySelector(`input#${this.parentId}_longitude`);

            const { addEventListener, getValue, setValue } = window.dynamicFields?.[this.getAttribute('dynamic-field-parent-id')];

            addEventListener('value', (value) => {
                this.updateValue(value);
            }, { init: true });

            [this.latitude, this.longitude].forEach(input =>
                input.addEventListener('input', () => {
                    const object = {
                        latitude: this.latitude.value,
                        longitude: this.longitude.value,
                    };
                    setValue(object);
                }));
            /* The initial value is set via a listener, alternatively, you can do this: */
            // this.updateValue(getValue());
        }

        updateValue(object) {
            this.latitude.value = object?.latitude;
            this.longitude.value = object?.longitude;
        }
    }
}
if (!window.customElements.get('geo-coordinates')) {
    window.customElements.define('geo-coordinates', window.GeoCoordinatesComponent);
}