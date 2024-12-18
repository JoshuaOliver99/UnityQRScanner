using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Events;
using ZXing;

public class ReadQRCode : MonoBehaviour
{
    [Tooltip("The AR camera used for matching projection settings.")]
    [SerializeField] private Camera ARCamera;

    [Tooltip("The camera used for QR code scanning.")]
    [SerializeField] private Camera QRCamera;

    [Tooltip("Event triggered when a QR code is detected.")]
    [SerializeField] private UnityEvent<string> OnQRCodeDetected;

    [Tooltip("Enable or disable continuous QR code scanning.")]
    [SerializeField] private bool continuousScanning = false;

    [Tooltip("The frequency that scanning occurs.")]
    [SerializeField] private float continuousScanningFrequency = 0.3f;


    private bool grabQR;
    private Texture2D qrTexture;
    private string lastScannedCode;

    /// <summary>
    /// Initializes the QR code reader.
    /// </summary>
    private void Start()
    {
        QRCamera.enabled = false;
        Application.runInBackground = true;

        if (OnQRCodeDetected == null)
        {
            OnQRCodeDetected = new UnityEvent<string>();
        }

        qrTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    }

    /// <summary>
    /// Updates the QR code reader state.
    /// </summary>
    private void Update()
    {
        if (QRButton.clicked || continuousScanning)
        {
            QRCamera.enabled = true;
            grabQR = true;
        }
    }

    /// <summary>
    /// Matches the Unity camera used for QR code scanning with the one used by ARFoundation.
    /// </summary>
    private void OnPreRender()
    {
        MatchCameraSettings();
    }

    /// <summary>
    /// Scans the QR code after rendering.
    /// </summary>
    private void OnPostRender()
    {
        if (grabQR)
        {
            StartCoroutine(ScanQRCodeWithDelay());
            grabQR = continuousScanning;
        }
    }

    /// <summary>
    /// Matches the camera settings between ARCamera and QRCamera.
    /// </summary>
    private void MatchCameraSettings()
    {
        QRCamera.projectionMatrix = ARCamera.projectionMatrix;
        QRCamera.fieldOfView = ARCamera.fieldOfView;
        QRCamera.transform.localPosition = Vector3.zero;
        QRCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    /// <summary>
    /// Scans the QR code using ZXing.
    /// </summary>
    private IEnumerator ScanQRCodeWithDelay()
    {
        ScanQRCode();
        yield return new WaitForSeconds(continuousScanningFrequency);
    }

    /// <summary>
    /// Scans the QR code using ZXing.
    /// </summary>
    private void ScanQRCode()
    {
        try
        {
            CaptureCameraTexture();
            DecodeQRCode();
        }
        catch (Exception e)
        {
            Debug.LogError("Error when capturing camera texture: " + e);
        }
    }

    /// <summary>
    /// Captures the camera texture.
    /// </summary>
    private void CaptureCameraTexture()
    {
        qrTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        qrTexture.Apply();
    }

    /// <summary>
    /// Decodes the QR code from the captured texture.
    /// </summary>
    private void DecodeQRCode()
    {
        IBarcodeReader barcodeReader = new BarcodeReader();
        var result = barcodeReader.Decode(qrTexture.GetPixels32(), qrTexture.width, qrTexture.height);
        if (result != null && result.Text != lastScannedCode)
        {
            Debug.Log("QR Text: " + result.Text);
            OnQRCodeDetected.Invoke(result.Text);
            lastScannedCode = result.Text;
        }
    }

    /// <summary>
    /// Cleans up resources when the script is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (qrTexture != null)
        {
            Destroy(qrTexture);
        }
    }
}