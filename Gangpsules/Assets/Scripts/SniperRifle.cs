using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperRifle : Gun
{
    [SerializeField]
    Transform laserPosition;

    [SerializeField]
    LineRenderer laser;

    private void Update()
    {
        if (equipped && CanFire())
        {
            RaycastHit hit;
            Vector3 laserEnd;
            if (Physics.Raycast(laserPosition.position, -transform.forward, out hit))
            {
                laserEnd = hit.point;
            }
            else
            {
                laserEnd = laserPosition.position - transform.forward * 100f;
            }

            laser.enabled = true;
            laser.SetPosition(0, laserPosition.position);
            laser.SetPosition(1, laserEnd);
            Gradient color = new Gradient();
            GradientColorKey[] colorKey = new GradientColorKey[1];
            GradientAlphaKey[] alphaKey = new GradientAlphaKey[1];
            if (userTag == "Player")
            {
                colorKey[0].color = Color.blue;
                alphaKey[0].alpha = 1f;
            
            }
            else
            {
                colorKey[0].color = Color.red;
                alphaKey[0].alpha = 1f;
            }
            color.SetKeys(colorKey, alphaKey);
            laser.colorGradient = color;
        }
        else
        {
            laser.enabled = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (equipped)
        {
            ///Rotate towards mouse
            Vector2 mousePos = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);
            Vector2 lookDirection = positionOnScreen - mousePos;
            float targetAngle = Mathf.Atan2(lookDirection.x, lookDirection.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
    }
}
