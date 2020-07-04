using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowderBoxScript : MonoBehaviour
{

    float speed = 20.0f; //how fast it shakes
    float amount = 0.1f; //how much it shakes

    private Vector3 initialPos;
    private Vector3 lastPos;

    [SerializeField]
    private MeshFormer coffeeMesh;
    [SerializeField]
    private float strength;

    private float shakeDuration = 1;
    private float shakeDeltaTime = 0;
    private bool isShaking = false;

    private Vector3 firstMousePos;
    private Vector3 collidedMousePos;
    private Vector3 collideExitMousePos;
    private bool isSwiping = false;
    private bool swipedOverCoffee = false;

    void Start()
    {
        initialPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        SwipeDetection();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isShaking)
                StartCoroutine(ShakeBox());
        }     
    }
   
    public IEnumerator ShakeBox()
    {
        //Kaba tap'lendiğinde kap titreyecek ve içindeki kahve düzleşecek.
        isShaking = true;
        float averageCoffeeHeight = coffeeMesh.FindMeshAverageHeight();
        while (shakeDeltaTime < shakeDuration)
        {
            shakeDeltaTime += Time.deltaTime;          
            transform.position = new Vector3(Mathf.Sin(Time.realtimeSinceStartup * speed) * amount, transform.position.y, transform.position.z);
            lastPos = transform.position;
            StartCoroutine(coffeeMesh.StraightenCoffee(averageCoffeeHeight));
            yield return null;
        }

        if (shakeDeltaTime > shakeDuration)
        {
            shakeDeltaTime = 0;
            while (shakeDeltaTime < shakeDuration - 0.5f)
            {
                shakeDeltaTime += Time.deltaTime;
                transform.position = Vector3.Lerp(lastPos, initialPos, 0.1f);
                yield return null;
            }
            shakeDeltaTime = 0;
            isShaking = false;
        }

    }

    private void SwipeDetection()
    {       
        //Swipe kahvenin üstünden en az bir kere geçerse çalışacak. Kahveye temas etmeyen swipeler geçersiz olacak.
        if (!isShaking)
        {
            
            if (Input.GetMouseButtonDown(0))
            {
                isSwiping = true;                
            }
            else if (Input.GetMouseButtonUp(0))
            {                                        
                isSwiping = false;
                swipedOverCoffee = false;
            }
            else
            {               
                if (isSwiping)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit) && hit.collider.name.Equals("CoffeePile"))
                    {                      
                            collidedMousePos = Input.mousePosition;
                            swipedOverCoffee = true;                       
                    }
                    else
                    {
                        if (swipedOverCoffee)
                        {
                            swipedOverCoffee = false;
                            collideExitMousePos = Input.mousePosition;
                            if (collideExitMousePos.x > collidedMousePos.x)
                                coffeeMesh.SwipeCoffee(true);
                            else
                                coffeeMesh.SwipeCoffee(false);
                        }
                    }
                }
            }
        }
    }
}
