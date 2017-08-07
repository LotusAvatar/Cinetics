using UnityEngine;
using System.Collections;

public class SataliteGlow : MonoBehaviour
{
    public Material ball;
    public Light redLight;
    public float speed = 1f;
    bool swithLight;
    float progress = 1f;
    float initialLightIntensity;
    Color nextColor;

    void Start()
    {
        initialLightIntensity = redLight.intensity;
    }

	// Update is called once per frame
	void Update ()
    {
        if (swithLight)
        {
            progress += Time.deltaTime * speed;
            if (progress >= 1f)
            {
                swithLight = !swithLight;
                progress = 1f;
            }
        }
        else
        {
            progress -= Time.deltaTime * speed;
            if (progress <= 0f)
            {
                swithLight = !swithLight;
                progress = 0f;
            }
        }
        redLight.intensity = initialLightIntensity * progress;
        nextColor = ball.color;
        nextColor.a = progress;
        ball.color = nextColor;
	}
}
