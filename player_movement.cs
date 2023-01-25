using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class player_movement : MonoBehaviour
{
    [Header("Vista")] public Transform camera;
    public Vector2 sensibility;

    [Header("Movimiento")] public float speed;

    private Rigidbody rb;
    private Vector3 escalaNormal;
    private Vector3 escalaAgachado;
    private bool agachado;
    public bool canMove;
    //public Vector2 RunAxis;
    //public Vector2 LookAxis;
    [Header("HeadBob")] public float bobbingSpeed = 0.05f;
    private float timer = 0f;

    public float bobbingAmount = 0.32f;
    public float midpoint = -0.01f;

    [Header("SFX")] public AudioSource audioSource;
    public GameObject sonidoPasos;
    private bool caminandoh, caminandov;
    public GameObject esquizofrenia;
    public AudioSource vocesEsquizofrenia;
    public AudioSource sonidoCorazon;

    [Header("Mecánicas")] public float medidorLocura = 0;
    public float medidorCardiaco = 90;
    public Animator bipbipAnimator;
    public Animator ACanimator;
    public Image locuraImage;
    public TMP_Text medidorCardiacoTEXT;
    private LevelLoader _levelLoader;

    [Header("Deteccion de enemigo")] public GameObject enemigo;
    private float angulodeVision = 60f;
    private float distanciadeVision = 80f;
    [SerializeField] private bool detectado;
    [SerializeField] private LayerMask LayerEnemigo;
    public Transform puntoDelRayo;
    public int enemigosenescena;

    [Header("Deteccion Objetos")] private Transform Camera;
    public LayerMask layerObjetos;
    public PostProcessVolume processVolume;
    [Header("Sotano")] public PuzzlePalancas puzzlePalancas;

    void Start()
    {
       
        processVolume.sharedProfile.GetSetting<AmbientOcclusion>().active = true;
        processVolume.sharedProfile.GetSetting<Bloom>().active = true;
        puzzlePalancas = FindObjectOfType<PuzzlePalancas>();
        camera = transform.Find("Main Camera");
        enemigosenescena = GameObject.FindGameObjectsWithTag("Enemy").Length;
        _levelLoader = FindObjectOfType<LevelLoader>();
        ACanimator.speed = 0;
        vocesEsquizofrenia = esquizofrenia.GetComponent<AudioSource>();
        actualizarMedidorLocura();
        canMove = true;
        caminandoh = false;
        caminandov = false;
        camera = transform.Find("Main Camera");
        Cursor.lockState = CursorLockMode.Locked;
        escalaNormal = transform.localScale;
        escalaAgachado = transform.localScale;
        escalaAgachado.y = escalaNormal.y * 0.6f;
        rb = GetComponent<Rigidbody>();
        audioSource = sonidoPasos.GetComponent<AudioSource>();
        puntoDelRayo = camera;
    }


    void Update()
    {
        interact();
        if (medidorLocura > 0)
        {
            medidorLocura -= Time.deltaTime / 3;
        }

        if (enemigosenescena > 0)
        {
            DetectarEnemigo();
        }

        if (detectado)
        {
            medidorLocura += 5 * Time.deltaTime;
        }

        actualizarMedidorLocura();
        if (medidorCardiaco >= 200)
        {
            _levelLoader.Corrutina();
        }

        actualizarMedidorCardiaco();
        bipbipAnimator.speed = medidorCardiaco / 100;

        if (medidorLocura > 30)
        {
            ACanimator.speed = (medidorCardiaco / 100) + 0.55f;
        }
        else
        {
            ACanimator.speed = 0;
        }

        if (canMove)
        {
            MovimientoVista();
            MovimientoJugador();
            Agacharse();
            if (Input.GetAxisRaw("Horizontal") > 0 || Input.GetAxis("Horizontal") < 0)
            {
                caminandoh = true;
                if (!caminandov)
                {
                    audioSource.Play();
                }
            }

            if (Input.GetAxisRaw("Horizontal") == 0)
            {
                caminandoh = false;
                audioSource.Pause();
            }

            if (Input.GetAxisRaw("Vertical") > 0 || Input.GetAxis("Vertical") < 0)
            {
                caminandov = true;
                if (!caminandoh)
                {
                    audioSource.Play();
                }
            }

            if (Input.GetAxisRaw("Vertical") == 0)
            {
                caminandov = false;

                audioSource.Pause();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!agachado && canMove)
        {
            HeadBob();
        }
    }

    void MovimientoVista()
    {
        float hor = Input.GetAxis("Mouse X");
        float ver = Input.GetAxis("Mouse Y");

        if (hor != 0)
        {
            transform.rotation *= Quaternion.Euler(Vector3.up * hor * sensibility.x);
        }

        if (ver != 0)
        {
            float angle = (camera.localEulerAngles.x - ver * sensibility.y + 360) % 360;
            if (angle > 180)
            {
                angle -= 360;
            }

            angle = Mathf.Clamp(angle, -80, 80);

            camera.localEulerAngles = Vector3.right * angle;
        }
    }

    void MovimientoJugador()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        if (agachado)
        {
            speed = 8f;
        }
        else
        {
            speed = 12f;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = 24f;
        }
        else
        {
            speed = 12f;
        }

        Vector3 motion = (transform.forward * ver + transform.right * hor).normalized;


        rb.velocity = motion * speed;
    }

    void Agacharse()
    {
        agachado = Input.GetKey(KeyCode.LeftControl);

        transform.localScale = Vector3.Lerp(transform.localScale, agachado ? escalaAgachado : escalaNormal, .1f);
    }

    void HeadBob()
    {
        float waveslice = 0.0f;
        float hor = Input.GetAxis("Horizontal");
        float ver = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            bobbingSpeed = 0.18f;
            bobbingAmount = 0.1f;
        }
        else
        {
            bobbingSpeed = 0.10f;
            bobbingAmount = 0.05f;
        }


        if (Mathf.Abs(hor) == 0 && Mathf.Abs(ver) == 0)
        {
            timer = 0.0f;
        }
        else
        {
            waveslice = Mathf.Sin(timer);
            timer = timer + bobbingSpeed;
            if (timer > Mathf.PI * 2)
            {
                timer = timer - (Mathf.PI * 2);
            }
        }

        if (waveslice != 0)
        {
            float translateChange = waveslice * bobbingAmount;

            float totalAxes2 = Mathf.Abs(hor) + Mathf.Abs(ver);

            float totalAxes = Mathf.Clamp(totalAxes2, 0.0f, 1.0f);

            translateChange = totalAxes * translateChange;

            camera.transform.localPosition =
                new Vector3((midpoint + translateChange) - 0.13f, midpoint + translateChange, 0f);
        }
        else
        {
            camera.transform.localPosition = new Vector3(0, midpoint, 0);
        }
    }

    public void actualizarMedidorLocura()
    {
        locuraImage.fillAmount = medidorLocura / 100;
        vocesEsquizofrenia.volume = (medidorLocura - 20) / 130;
    }

    void actualizarMedidorCardiaco()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            medidorCardiaco += Time.fixedDeltaTime / 2.4f + ((medidorLocura / 100) / 2.4f);
        }
        else
        {
            if (medidorCardiaco >= 90.5f)
            {
                medidorCardiaco -= Time.fixedDeltaTime / 1.5f;
            }
        }

        int enteroCardiaco = (int) medidorCardiaco;
        medidorCardiacoTEXT.text = enteroCardiaco.ToString();
        sonidoCorazon.volume = (medidorCardiaco / 500) - 0.05f;
        sonidoCorazon.pitch = medidorCardiaco / 100;
    }

    void DetectarEnemigo()
    {
        if (enemigo == null)
        {
            enemigo = GameObject.FindWithTag("Enemy");
            enemigosenescena--;
        }

        Vector3 playerVector = enemigo.transform.position - puntoDelRayo.position;
 
        Debug.DrawRay(puntoDelRayo.position, playerVector * 80f, Color.magenta);
        if (Vector3.Angle(transform.forward, playerVector) < angulodeVision &&
            playerVector.magnitude < distanciadeVision)
        {
            if (Physics.Raycast(puntoDelRayo.position, playerVector, distanciadeVision, LayerEnemigo))
            {
              
                detectado = true;
            }
        }
        else
        {
            detectado = false;
        }
    }

    void interact()
    {
        RaycastHit objectRaycastHit;
        if (Physics.Raycast(camera.position, camera.forward, out objectRaycastHit, 10f, layerObjetos))
        {
            if (objectRaycastHit.collider.CompareTag("Palanca") && Input.GetKeyDown(KeyCode.E))
            {
                Palanca palanca = objectRaycastHit.collider.GetComponent<Palanca>();
                if (palanca.activa == false)
                {
                    bool actual = palanca.activa;
                    palanca.activa = !actual;
                    if (!palanca.puedeComprobar || !palanca.reiniciar)
                    {
                        int num = palanca.numero;
                        puzzlePalancas.addLever(num);
                    }
                }
            }
        }
    }
}