using UnityEngine;

public class CameraOptimization : MonoBehaviour
{
    public  Camera mainCam;
    void Start()
    {
        // 1. Lấy component Camera đang gắn trên chính MainCamera này
        

        // 2. Tạo một mảng chứa 32 giá trị khoảng cách (Tương ứng 32 Layer của Unity)
        float[] distances = new float[32];

        // 3. Thiết lập khoảng cách ẩn (bằng mét) cho Layer số 10 (SmallProps)
        // Thay số 10 bằng số ô Layer bạn đã đặt ở Bước 1 nếu bạn đặt ô khác
        distances[11] = 15f; 

        // 4. Kích hoạt ma trận khoảng cách này vào hệ thống lọc của Camera
        mainCam.layerCullDistances = distances;
    }
}