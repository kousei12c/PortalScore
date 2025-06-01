using UnityEngine;

public class BouncItem : MonoBehaviour
{

    private Rigidbody2D rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();


        // 回転を止める
        rb.freezeRotation = true;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;

        switch (tag)
        {
            case "Reflector":
                Reflect(collision);
                break;

            case "OtherTag":
                // 他のタグの処理
                break;
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        string tag = other.gameObject.tag;

        switch(tag)
        {   
            case "SpeedBoost":
                SpeedBoost(other.gameObject);
                break;
      }
    }

    void Reflect(Collision2D collision)
    {
        Vector2 inVelocity = rb.linearVelocity;
        Vector2 normal = collision.contacts[0].normal;
        Vector2 reflectVelocity = Vector2.Reflect(inVelocity, normal);
        rb.linearVelocity = reflectVelocity;
    }

    void SpeedBoost(GameObject boostObject)
    {
        rb.linearVelocity *= 1.5f;
        Destroy(boostObject);
    }

}