using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class paintable : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    
    public void ApplyPaint(Vector3 position, float innerRadius, float outerRadius, Color color) {
        Vector3 center = transform.InverseTransformPoint(position);
        float outerR = transform.InverseTransformVector(outerRadius * Vector3.right).magnitude;
        float innerR = innerRadius * outerR / outerRadius;
        float innerRsqr = innerR * innerR;
        float outerRsqr = outerR * outerR;
        float tFactor = 1f / (outerR - innerR);
        Vector3[] verts = mesh.vertices;
        Color32[] vertColors = mesh.colors32;
      
        for (int i=0; i<verts.Length; i++) {
            Vector3 delta = verts[i] - center;
            float dsqr = delta.sqrMagnitude;
            if (dsqr > outerRsqr) continue;
            int a = vertColors[i].a;
            vertColors[i] = color;
            if (dsqr < innerRsqr) vertColors[i].a = 255;
            else {
                float d = Mathf.Sqrt(dsqr);
                byte blobA = (byte)(255 - 255 * (d - innerR)*tFactor);
                if (blobA >= a) vertColors[i].a = blobA;
            }
        }
        mesh.colors32 = vertColors;
    }
}
