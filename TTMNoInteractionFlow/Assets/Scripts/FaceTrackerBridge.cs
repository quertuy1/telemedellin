using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// Puente entre Unity y el servidor Python de Inteligencia Artificial.
/// Envia frames de la webcam en baja resolucion y recibe las coordenadas
/// del rostro para controlar los accesorios AR.
/// </summary>
public class FaceTrackerBridge : MonoBehaviour
{
    [Header("Conexion Python AI")]
    public int sendPort = 5005;
    public int receivePort = 5006;
    public float sendRate = 0.05f; // 20 fps

    [Header("Procesamiento")]
    public int downscaleWidth = 320;
    public int downscaleHeight = 180;
    [Range(10, 100)] public int jpgQuality = 40;

    private UdpClient udpSender;
    private UdpClient udpReceiver;
    private Thread receiveThread;
    private bool isRunning = true;

    // Estado del rostro detectado (actualizado por el thread de recepcion)
    private float targetFaceX = 0.5f;
    private float targetFaceY = 0.5f;
    private float targetFaceScale = 0.2f;
    private bool faceDetected = false;

    // Buffer para extraer la imagen de la webcam
    private Texture2D downscaleTex;
    private float nextSendTime = 0f;

    [System.Serializable]
    public class FaceData
    {
        public float x;
        public float y;
        public float scale;
    }

    void Start()
    {
        // Inicializar sockets
        udpSender = new UdpClient();
        
        udpReceiver = new UdpClient(receivePort);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        downscaleTex = new Texture2D(downscaleWidth, downscaleHeight, TextureFormat.RGB24, false);
        
        Debug.Log("FaceTrackerBridge: Iniciado puente con IA. Asegurate de correr el script de Python.");
    }

    void Update()
    {
        if (Time.time >= nextSendTime)
        {
            SendFrameToAI();
            nextSendTime = Time.time + sendRate;
        }

        // Actualizar el ARItemController si existe y esta en modo automatico
        ARItemController arItem = Object.FindFirstObjectByType<ARItemController>();
        if (arItem != null && arItem.useFaceTracking)
        {
            if (faceDetected)
            {
                arItem.UpdateFromFaceTracking(targetFaceX, targetFaceY, targetFaceScale);
            }
        }
    }

    private void SendFrameToAI()
    {
        CameraManager camMgr = Object.FindFirstObjectByType<CameraManager>();
        if (camMgr == null) return;

        WebCamTexture webcam = camMgr.GetWebcamTexture();
        if (webcam == null || !webcam.isPlaying || webcam.width <= 16) return;

        // Copiar y redimensionar el frame para la IA (muy rapido y liviano)
        // Guardamos los pixeles actuales
        Color32[] pixels = webcam.GetPixels32();
        
        // Forma basica de downscaling por sampleo
        int w = webcam.width;
        int h = webcam.height;
        Color[] downscaled = new Color[downscaleWidth * downscaleHeight];
        
        for (int y = 0; y < downscaleHeight; y++)
        {
            for (int x = 0; x < downscaleWidth; x++)
            {
                int srcX = Mathf.FloorToInt((float)x / downscaleWidth * w);
                int srcY = Mathf.FloorToInt((float)y / downscaleHeight * h);
                downscaled[y * downscaleWidth + x] = pixels[srcY * w + srcX];
            }
        }

        downscaleTex.SetPixels(downscaled);
        downscaleTex.Apply();

        // Comprimir a JPG calidad baja para mandar ultra rapido
        byte[] jpgData = downscaleTex.EncodeToJPG(jpgQuality);
        
        try
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sendPort);
            udpSender.Send(jpgData, jpgData.Length, endPoint);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error enviando frame a Python: {e.Message}");
        }
    }

    private void ReceiveData()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        while (isRunning)
        {
            try
            {
                byte[] data = udpReceiver.Receive(ref anyIP);
                string json = Encoding.UTF8.GetString(data);
                
                FaceData face = JsonUtility.FromJson<FaceData>(json);
                targetFaceX = face.x;
                targetFaceY = face.y;
                targetFaceScale = face.scale;
                faceDetected = true;
            }
            catch (SocketException)
            {
                // Normal al cerrar
            }
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        
        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Abort();
            
        if (udpReceiver != null)
            udpReceiver.Close();
            
        if (udpSender != null)
            udpSender.Close();
            
        if (downscaleTex != null)
            Destroy(downscaleTex);
    }
}
