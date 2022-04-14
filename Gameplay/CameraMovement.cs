using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Die Klasse CameraMovement wird dazu verwendet, um die Kamera dem Spieler möglichst smooth folgen zu lassen. 
Sie muss an ein Kamera GameObject gebunden werden. Desto kleiner der Smoothing Wert (zum Beispiel 0.1), um so langsamer zieht die Kamera nach.*/

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float smoothing;

    public Vector2 maxPosition;
    public Vector2 minPosition;

    void Start()
    {
        
    }

/*LateUpdate wird ganz zum Ende eines Frames ausgeführt. 
Der Spieler soll sich innerhalb des regulären Updates bewegen und die Kamera soll im LateUpdate hinterherziehen. 
Wenn beide zusammen updaten, könnte dies zu Kamera ruckeln führen.

lerp nutzt lineare Interpolation, um die Distanz zwischen dem Target und der Kamera langsam in wenig % anzugleichen.
Der Z-Vektor von targetPosition wird festgesetzt, damit die Kamera nicht in der Spielebene verschwindet.
*/

    void LateUpdate()
    {
        if(transform.position != target.position)
        {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

            targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);
            
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing);
        }
    }
}
