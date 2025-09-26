if (!window.PaintComponent) {
    window.PaintComponent = class PaintComponent extends HTMLElement {
        constructor() {
            super();
            const shadow = this.attachShadow({ mode: 'open' });

            shadow.innerHTML = `
          <style>
            canvas {
              border: 1px solid #ccc;
              display: block;
              margin-bottom: 10px;
            }
            .controls {
              display: flex;
              flex-wrap: wrap;
              gap: 10px;
              align-items: center;
            }
            button, input, select {
              padding: 5px 10px;
              font-size: 14px;
              cursor: pointer;
            }
          </style>
          <div>
            <canvas id="canvas" width="800" height="400"></canvas>
            <div class="controls">
              <button id="clearBtn">Clear</button>
              <label>Color:
                <input type="color" id="colorPicker" value="#000000">
              </label>
            </div>
          </div>
        `;
        }

        connectedCallback() {
            const shadow = this.shadowRoot;
            const canvasEl = shadow.getElementById('canvas');
            const clearBtn = shadow.getElementById('clearBtn');
            const colorPicker = shadow.getElementById('colorPicker');
            this.loadingFromJson = false;

            this.canvas = new fabric.Canvas(canvasEl);
            this.canvas.isDrawingMode = true;
            this.canvas.freeDrawingBrush.width = 3;
            this.canvas.freeDrawingBrush.color = colorPicker.value;

            const saveState = () => {
                if (this.loadingFromJson) {
                    return;
                }
                const json = this.canvas.toJSON();
                const dataURL = this.canvas.toDataURL({
                    format: 'png',
                    quality: 1.0
                });

                const object = {
                    json,
                    dataURL
                };

                this.onChange(object);
            };

            this.canvas.on('object:added', saveState);
            this.canvas.on('object:modified', saveState);
            this.canvas.on('object:removed', saveState);

            colorPicker.addEventListener('input', () => {
                this.canvas.freeDrawingBrush.color = colorPicker.value;
            });

            clearBtn.addEventListener('click', () => {
                this.canvas.clear();
            });
        }

        loadFromJson(json) {
            this.loadingFromJson = true;
            this.canvas.loadFromJSON(json, () => {
                this.canvas.renderAll();
                this.loadingFromJson = false;
            });
        }

        onChange(object) {
            // leave it empty
        }
    }
}
if (!window.customElements.get('paint-component')) {
    window.customElements.define('paint-component', window.PaintComponent);
}