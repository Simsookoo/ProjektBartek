using UnityEngine;

namespace Assets
{
    public class ColliderGizmos2D : MonoBehaviour
    {
        public Color gizmoColor = Color.green;
        public float alphaChannel = 0.6f;

        private void OnDrawGizmos()
        {
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
            var capsuleColliders = GetComponents<CapsuleCollider2D>();

            if (boxCollider != null && boxCollider.enabled)
                DrawBoxCollider(boxCollider);

            if (polygonCollider != null && polygonCollider.enabled)
                DrawPolygonCollider(polygonCollider);


            if (capsuleColliders != null)
            {
                foreach (var capsuleCollider in capsuleColliders)
                {
                    if (capsuleCollider.enabled)
                        DrawCapsuleCollider(capsuleCollider, 4);
                }
            }
        }

        private void DrawBoxCollider(BoxCollider2D boxCollider)
        {
            Gizmos.color = gizmoColor;
            Vector3 position = transform.TransformPoint(boxCollider.offset);
            Vector3 size = new Vector3(boxCollider.size.x * transform.lossyScale.x, boxCollider.size.y * transform.lossyScale.y, 1f);

            Gizmos.DrawWireCube(position, size);

            Color fillColor = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, alphaChannel);
            Gizmos.color = fillColor;
            Gizmos.DrawCube(position, size);
        }

        private void DrawPolygonCollider(PolygonCollider2D polygonCollider)
        {
            Gizmos.color = gizmoColor;

            for (int i = 0; i < polygonCollider.pathCount; i++)
            {
                Vector2[] points = polygonCollider.GetPath(i);
                Vector3[] worldPoints = new Vector3[points.Length];
                for (int j = 0; j < points.Length; j++)
                {
                    worldPoints[j] = transform.TransformPoint(points[j]);
                }
                for (int j = 0; j < points.Length; j++)
                {
                    //Gizmos.DrawLine(worldPoints[j], worldPoints[(j + 1) % points.Length]);
                    GizmosUtils.DrawLine(worldPoints[j], worldPoints[(j + 1) % points.Length], 5);
                }
            }
        }

        private void DrawCapsuleCollider(CapsuleCollider2D capsuleCollider, float lineWidth = 0f)
        {
            Gizmos.color = gizmoColor;

            // Pobieranie transformacji i wymiarów
            Vector3 position = transform.position + new Vector3(capsuleCollider.offset.x * transform.lossyScale.x,
                                                              capsuleCollider.offset.y * transform.lossyScale.y, 0);

            // Wymiary z uwzględnieniem skali obiektu
            Vector2 size = new Vector2(
                capsuleCollider.size.x * transform.lossyScale.x,
                capsuleCollider.size.y * transform.lossyScale.y
            );

            // Sprawdzamy czy kapsuła jest pionowa czy pozioma
            bool isVertical = capsuleCollider.direction == CapsuleDirection2D.Vertical;

            // Pobieramy promień i wysokość w zależności od kierunku
            float radius, height;
            if (isVertical)
            {
                radius = size.x * 0.5f;
                height = size.y;
            }
            else
            {
                radius = size.y * 0.5f;
                height = size.x;
            }

            // Długość prostych części (bez półokręgów)
            float capsuleBodyLength = Mathf.Max(0, height - 2 * radius);

            // Kąt obrotu obiektu w radianach
            float baseRotation = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;

            // Punkty środkowe półokręgów
            Vector3 upperCenter, lowerCenter;
            if (isVertical)
            {
                upperCenter = new Vector3(position.x, position.y + capsuleBodyLength * 0.5f, position.z);
                lowerCenter = new Vector3(position.x, position.y - capsuleBodyLength * 0.5f, position.z);
            }
            else
            {
                upperCenter = new Vector3(position.x + capsuleBodyLength * 0.5f, position.y, position.z);
                lowerCenter = new Vector3(position.x - capsuleBodyLength * 0.5f, position.y, position.z);
            }

            // Obracamy punkty wokół centralnej pozycji
            upperCenter = RotatePointAroundPivot(upperCenter, position, baseRotation);
            lowerCenter = RotatePointAroundPivot(lowerCenter, position, baseRotation);

            // Punkty prostokąta głównego
            Vector3 topLeft, topRight, bottomLeft, bottomRight;
            if (isVertical)
            {
                topLeft = new Vector3(position.x - radius, position.y + capsuleBodyLength * 0.5f, position.z);
                topRight = new Vector3(position.x + radius, position.y + capsuleBodyLength * 0.5f, position.z);
                bottomLeft = new Vector3(position.x - radius, position.y - capsuleBodyLength * 0.5f, position.z);
                bottomRight = new Vector3(position.x + radius, position.y - capsuleBodyLength * 0.5f, position.z);
            }
            else
            {
                topLeft = new Vector3(position.x - capsuleBodyLength * 0.5f, position.y + radius, position.z);
                topRight = new Vector3(position.x + capsuleBodyLength * 0.5f, position.y + radius, position.z);
                bottomLeft = new Vector3(position.x - capsuleBodyLength * 0.5f, position.y - radius, position.z);
                bottomRight = new Vector3(position.x + capsuleBodyLength * 0.5f, position.y - radius, position.z);
            }

            // Obracamy punkty prostokąta
            topLeft = RotatePointAroundPivot(topLeft, position, baseRotation);
            topRight = RotatePointAroundPivot(topRight, position, baseRotation);
            bottomLeft = RotatePointAroundPivot(bottomLeft, position, baseRotation);
            bottomRight = RotatePointAroundPivot(bottomRight, position, baseRotation);

            // Rysujemy prostokąt
            GizmosUtils.DrawLine(topLeft, topRight, lineWidth);
            GizmosUtils.DrawLine(topRight, bottomRight, lineWidth);
            GizmosUtils.DrawLine(bottomRight, bottomLeft, lineWidth);
            GizmosUtils.DrawLine(bottomLeft, topLeft, lineWidth);

            // Rysujemy półokręgi
            if (isVertical)
            {
                DrawArc(upperCenter, radius, -180, 0, baseRotation, lineWidth);
                DrawArc(lowerCenter, radius, 0, 180, baseRotation, lineWidth);
            }
            else
            {
                DrawArc(upperCenter, radius, -90, 90, baseRotation, lineWidth);
                DrawArc(lowerCenter, radius, 90, 270, baseRotation, lineWidth);
            }
        }

        // Pomocnicza metoda do rysowania łuku
        private void DrawArc(Vector3 center, float radius, float startAngle, float endAngle, float rotation, float lineWidth)
        {
            int segments = 20;
            float angleStep = (endAngle - startAngle) / segments;

            Vector3 prevPoint = Vector3.zero;
            bool firstPoint = true;

            for (int i = 0; i <= segments; i++)
            {
                float angle = (startAngle + angleStep * i) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;

                Vector3 point = center + RotateVector(new Vector3(x, y, 0), rotation);

                if (!firstPoint)
                {
                    GizmosUtils.DrawLine(prevPoint, point, lineWidth);
                }

                prevPoint = point;
                firstPoint = false;
            }
        }

        // Pomocnicza metoda do obracania wektora względem punktu pivot
        private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle)
        {
            Vector3 dir = point - pivot;
            dir = RotateVector(dir, angle);
            return pivot + dir;
        }

        // Pomocnicza metoda do obracania wektora
        private Vector3 RotateVector(Vector3 vector, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            float x = vector.x * cos - vector.y * sin;
            float y = vector.x * sin + vector.y * cos;

            return new Vector3(x, y, vector.z);
        }
    }
}