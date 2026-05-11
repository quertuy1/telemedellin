# Proyecto Telemedellín - Photo Booth con IA (Flujo Interactivo)

Este repositorio contiene el proyecto en Unity y el código en Python para una experiencia de *Photo Booth* interactiva, diseñada para cambiar entre distintas épocas (50s, 70s, 90s y Actualidad), aplicando filtros visuales, sonoros y accesorios en Realidad Aumentada (AR) mediante Inteligencia Artificial.

## Características Principales

1. **Filtros Visuales por Época (URP Volume Profiles):**
   - Transiciones de color y texturas utilizando el sistema de Post-Processing de Universal Render Pipeline (URP).
   - Se gestionan automáticamente al seleccionar un botón en la interfaz mediante el `FilterManager.cs`.

2. **Accesorios AR (Props 2D):**
   - Sprites 2D como sombreros, corbatines y gafas que se añaden a la escena.
   - El sistema enciende y apaga los accesorios correspondientes según la época elegida (`FilterManager.cs`).
   - Siguen el rostro del usuario en tiempo real mediante el `ARItemController.cs` gracias a las coordenadas enviadas por el servidor de Inteligencia Artificial.

3. **Inteligencia Artificial (Tracking Facial):**
   - Servidor local en Python (`face_tracker_server.py`) basado en MediaPipe que detecta el rostro del usuario.
   - `FaceTrackerBridge.cs` captura la webcam en Unity, manda los frames en baja resolución a Python por UDP y recibe las coordenadas (X, Y y Escala) instantáneamente.
   - ¡Soporta el tracking simultáneo de múltiples accesorios al mismo tiempo!

4. **Diseño de Interfaz e Interacción:**
   - Botones con animaciones fluidas (`EraButtonHandler.cs`) para cambiar la experiencia en un solo clic.

---

## Estructura de Archivos Clave

- `TTMNoInteractionFlow/Assets/Scenes/Foto1.unity`: Escena principal donde corre toda la experiencia.
- `TTMNoInteractionFlow/Assets/Scripts/`: Todos los controladores C# del proyecto.
- `TTMNoInteractionFlow/Assets/Filters/Props/`: Imágenes PNG que sirven de accesorios 2D.
- `TTMNoInteractionFlow/face_tracker_server.py`: Script de IA en Python.
- `INICIAR_IA_SOMBREROS.bat`: Archivo ejecutable para correr la IA con un clic.

---

## Instrucciones de Instalación y Uso

### Requisitos
- **Unity 6 (6000.x)** o superior con Universal Render Pipeline (URP).
- **Python 3.8+** instalado en el sistema.
- Librerías de Python requeridas: `cv2` (OpenCV) y `mediapipe`.

### Pasos para probar la experiencia completa:

1. **Clonar y Abrir:**
   - Clona este repositorio y abre la carpeta `TTMNoInteractionFlow` en Unity Hub.
   
2. **Iniciar el Servidor de IA:**
   - Antes de darle Play a Unity, haz doble clic en el archivo **`INICIAR_IA_SOMBREROS.bat`** (ubicado en la raíz del repositorio). Esto abrirá una consola negra que dice "Servidor UDP de Face Tracking iniciado en puerto 5006". ¡Déjala abierta!
   
3. **Iniciar en Unity:**
   - Abre la escena `Foto1`.
   - Dale al botón **Play**.
   - Haz clic en los botones de época en la interfaz. Verás cómo cambia el color de tu cámara, suenan los audios y los accesorios 2D aparecen y persiguen tu rostro mágicamente.

### Modo "Sin Tracking" (Opcional)
Si quieres probar la interfaz sin que los objetos sigan tu rostro (moviéndolos con el mouse en lugar de tu cara):
1. No abras el archivo `.bat`.
2. Selecciona tus accesorios en la escena de Unity y en el script `ARItemController`, desmarca la casilla **"Use Face Tracking"**.

---

## Ajuste de Accesorios (Para Desarrolladores)

Si sientes que un accesorio está muy arriba, muy abajo o es muy grande:
- Selecciona el objeto en la jerarquía (ej. `50s sombrero`).
- En el componente `ARItemController`:
  - Modifica el **Tracking Offset** para subirlo (Y positivo) o bajarlo (Y negativo). Ejemplo: Para un corbatín usa `Y = -0.8`.
  - Modifica el **Face Scale Multiplier** para hacerlo más grande o más pequeño (por defecto para imágenes 2D suele ser alrededor de `5` o `10`).
