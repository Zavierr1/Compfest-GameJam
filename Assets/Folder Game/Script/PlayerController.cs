using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float kecepatanGerak = 5f;
    private Rigidbody rb;
    private Vector3 arahGerak;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Input gerakan
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        arahGerak = new Vector3(horizontal, 0, vertical).normalized;

        // Input perubahan gravitasi
        if (Input.GetKeyDown(KeyCode.Q)) UbahGravitasi(-90);
        if (Input.GetKeyDown(KeyCode.E)) UbahGravitasi(90);
    }

    void FixedUpdate()
    {
        // Terapkan gerakan
        Vector3 gerakan = transform.TransformDirection(arahGerak) * kecepatanGerak;
        rb.linearVelocity = new Vector3(gerakan.x, rb.linearVelocity.y, gerakan.z);
    }

    void UbahGravitasi(float sudut)
    {
        transform.Rotate(Vector3.forward, sudut);
        Physics.gravity = -transform.up * 9.81f;
        
        // Efek visual sederhana (ganti warna sementara)
        GetComponent<Renderer>().material.color = Color.yellow;
        Invoke("KembaliWarnaNormal", 0.2f);
    }

    void KembaliWarnaNormal()
    {
        GetComponent<Renderer>().material.color = Color.green;
    }
}