using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshFormer : MonoBehaviour
{
    public bool recalculateNormals;  
    public bool collisionDetection;

    Mesh mesh;
    public MeshCollider meshCollider;
    List<Vector3> vertices;

    [SerializeField]
    private ParticleSystem coffeeFlowParticle;
    [SerializeField]
    private ParticleSystem coffeeSwipeParticle;

    public float strength = 0.0002f;

    private float avgHeightBeforeShake;


    



    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
        vertices = mesh.vertices.ToList();
        coffeeSwipeParticle = GetComponent<ParticleSystem>();
    }

    public void AddOrSubstractCoffee(Vector3 direction,float strength)
    {      
        //Kodu olası kahve eksiltme durumunu da düşünerek yazdım. Ama hiç eksiltecek şekilde kullanmadım.
        //Kahve birikintisi belirli bir seviyenin altında ise, eksiltme işlemi yapılmasın.
        if (!(meshCollider.bounds.size.y<0.1f && direction == Vector3.back))
        {            
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vi = transform.TransformPoint(vertices[i]);
                float distanceX = transform.position.x - vi.x;
                float distanceY = transform.position.z - vi.z;
                float magnitudeXY = Mathf.Sqrt(Mathf.Pow(distanceX, 2) + Mathf.Pow(distanceY, 2));
                magnitudeXY = Random.Range(magnitudeXY - 0.11f, magnitudeXY + 0.2f);

                //Belirli vertice noktalarına, gürültülü bir şekilde kahve eklentisi yapılsın
                if (magnitudeXY < 0.1f)
                {
                    vertices[i] += direction * strength * Random.Range(0.5f, 3f);
                }
                else if (magnitudeXY >= 0.1f && magnitudeXY < 0.2f)
                {
                    vertices[i] += direction * strength * Random.Range(0.5f, 2f);
                }
                else if (magnitudeXY > 0.2f && magnitudeXY < 0.35f)
                {
                    vertices[i] += direction * strength * Random.Range(0.1f, 1.5f);
                }
            }

            if (recalculateNormals)
                mesh.RecalculateNormals();

            if (collisionDetection)
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh;
            }
            mesh.SetVertices(vertices);
        }
            

    }
   
    public IEnumerator StraightenCoffee(float average)
    {
        int a = 0;

        //Kap her sallandığında, kahve biraz düzlenecek. Kahve yüksekliğine bağlı olarak kahvenin tamamen düzleşmesi için bir veya birkaç sallama gerekebilir.
        while (a<4)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 verVec = vertices[i];               
                vertices[i] = Vector3.Lerp(verVec, new Vector3(vertices[i].x, vertices[i].y, average + Random.Range(-0.009f, 0.009f)), 0.005f);               
            }
            a++;
            yield return new WaitForEndOfFrame();
        }
        
        if (recalculateNormals)
            mesh.RecalculateNormals();

        if (collisionDetection)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
        mesh.SetVertices(vertices);

    }

    public void SwipeCoffee(bool isRightSwiped)
    {
        //Swipe yönüne ve swipe anındaki tepecik yüksekliğine bağlı olarak kahve partiküllerinin yön ve adedi değişecek. Sonrasında ise kahve tepeciği kap yüksekliğine gelecek.
        bool isSwiping = false;
        float heightDif=0;
        float afterSwipeHeight = transform.parent.transform.GetComponent<MeshCollider>().bounds.size.y / 2.5f;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vi = transform.TransformPoint(vertices[i]);
            if (vi.y>afterSwipeHeight)
            {
                vertices[i] = new Vector3(vertices[i].x, vertices[i].y, afterSwipeHeight);
                if((vi.y- afterSwipeHeight)>heightDif)
                    heightDif = vi.y - afterSwipeHeight;               
                isSwiping = true;
            }          
        }

        if (isSwiping)
        {
            var shapeModule = coffeeSwipeParticle.shape;
            var emissionModule = coffeeSwipeParticle.emission;
            shapeModule.position = new Vector3(coffeeSwipeParticle.shape.position.x, coffeeSwipeParticle.shape.position.y, meshCollider.bounds.size.y / 2);
            emissionModule.rateOverTime = 150 * heightDif;

            if (isRightSwiped)              
                shapeModule.rotation = new Vector3(coffeeSwipeParticle.shape.rotation.x, coffeeSwipeParticle.shape.rotation.y, 300);
            else
                shapeModule.rotation = new Vector3(coffeeSwipeParticle.shape.rotation.x, coffeeSwipeParticle.shape.rotation.y, 120);

            coffeeSwipeParticle.Play();            
        }

        if (recalculateNormals)
            mesh.RecalculateNormals();

        if (collisionDetection)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
        mesh.SetVertices(vertices);
        isSwiping = false;
        heightDif = 0;

    }

    private void FixedUpdate()
    {
        if (coffeeFlowParticle.isPlaying && meshCollider.bounds.size.y<1)
        {
            AddOrSubstractCoffee(Vector3.forward,strength);
        }
        
    }

    private float totalHeight = 0;
    public float FindMeshAverageHeight()
    {
        float average;       
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vi = transform.TransformPoint(vertices[i]);
            totalHeight += vi.y-transform.position.y;
        }
        average = totalHeight / vertices.Count;
        totalHeight = 0;       
        return average;
    }

}
