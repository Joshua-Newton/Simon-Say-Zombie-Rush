using UnityEngine;

public class DisplaySizeDetector : MonoBehaviour
{
    void Start()
    {
        DetectDisplaySize();
    }

    void DetectDisplaySize()
    {
        // Get the screen resolution in pixels
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // Get the DPI of the device
        float dpi = Screen.dpi;

        // Fallback if DPI is not available (returns 0)
        if (dpi == 0)
        {
            Debug.LogWarning("DPI not available, using default DPI for estimate.");
            // Default DPI values for desktop monitors and TVs
            dpi = GetDefaultDpiEstimate();
        }

        // Calculate the width and height of the screen in inches
        float screenWidthInInches = screenWidth / dpi;
        float screenHeightInInches = screenHeight / dpi;

        // Calculate the diagonal size of the screen in inches
        float screenDiagonalInInches = Mathf.Sqrt(Mathf.Pow(screenWidthInInches, 2) + Mathf.Pow(screenHeightInInches, 2));

        // Output the information to the console
        Debug.Log($"Screen Resolution: {screenWidth}x{screenHeight} pixels");
        Debug.Log($"DPI: {dpi}");
        Debug.Log($"Screen Size: {screenWidthInInches:F2} x {screenHeightInInches:F2} inches");
        Debug.Log($"Diagonal Screen Size: {screenDiagonalInInches:F2} inches");

        // Categorize the device based on the screen size
        CategorizeDeviceBySize(screenDiagonalInInches);
    }

    float GetDefaultDpiEstimate()
    {
        // Default DPI estimates for desktop monitors and TVs
        // Desktop monitors typically have a DPI between 90 and 110
        // TVs typically have a DPI between 50 and 100

        // Since we are not considering mobile devices, use default values for desktop and TV
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            return 96f; // Estimate for desktop monitors
        }
        else
        {
            return 72f; // Estimate for TVs or fallback
        }
    }

    void CategorizeDeviceBySize(float diagonalSize)
    {
        // Categorize devices based on the diagonal screen size
        if (diagonalSize < 12f)
        {
            Debug.Log("Device Type: Small monitor");
        }
        else if (diagonalSize >= 12f && diagonalSize < 30f)
        {
            Debug.Log("Device Type: Medium monitor");
        }
        else if (diagonalSize >= 30f && diagonalSize < 55f)
        {
            Debug.Log("Device Type: Large monitor or small TV");
        }
        else
        {
            Debug.Log("Device Type: Large TV");
        }
    }
}
