using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceControl : MonoBehaviour
{
    [SerializeField] GameObject raceTagPrefab;
    [SerializeField] Text positionText;

    int playerNumber = -1;

    [System.Serializable]
    public class Participant
    {
        public RaceTag tag;
        public int number;
        public int points;
        public int lastCheckpoint;
    }
    public List<Participant> participants = new List<Participant>();

    [SerializeField] Material redLight;
    [SerializeField] Material yellowLight;
    [SerializeField] Material greenLight;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip horn;

    //CarControl[] cars;
    //int[] points;
    //int[] lastCheckpoint;
    //int[] standing;
    void Awake()
    {
        redLight.DisableKeyword("_EMISSION");
        yellowLight.DisableKeyword("_EMISSION");
        greenLight.DisableKeyword("_EMISSION");
    }

    // Start is called before the first frame update
    void Start()
    {


        CarControl[] cars = FindObjectsOfType<CarControl>();
        //points = new int[cars.Length];
        //lastCheckpoint = new int[cars.Length];
        //standing = new int[cars.Length];

        for(int i = 0; i < cars.Length; i++)
        {
            //Debug.Log("Making Race Tag for "+ i.ToString());
            RaceTag tag = cars[i].GetComponentInChildren<RaceTag>();
            if(tag == null)
                tag = Instantiate(raceTagPrefab, cars[i].transform.position, cars[i].transform.rotation, cars[i].transform).GetComponent<RaceTag>();
            tag.SetNumber(i);

            Participant p = new Participant();
            p.tag = tag;
            p.number = i;
            p.points = 0;
            p.lastCheckpoint = -5;
            participants.Add(p);

            if(cars[i].driverSeat.occupant != null && GameControl.instance.player.possessed != null && cars[i].driverSeat.occupant == GameControl.instance.player.possessed)
            {
                playerNumber = i;
            }

            cars[i].blockMovement = true;
        }

        UpdatePointText();

        StartCoroutine(StartRace());
    }

    public void AddPoint(RaceTag tag, int checkpoint)
    {
        int index = 0;
        for(int i = 0; i < participants.Count; i++)
        {
            if(participants[i].tag == tag)
            {
                index = i;
                break;
            }
        }

        int delta = checkpoint - participants[index].lastCheckpoint;
        participants[index].lastCheckpoint = checkpoint;
        if(delta == 1 || Mathf.Abs(delta) >= 2)
        {
            participants[index].points += 1;
        }

        UpdatePointText();
    }

    public void AddPoint(int number, int checkpoint)
    {
        if(number >= participants.Count) return;

        int index = 0;
        for(int i = 0; i < participants.Count; i++)
        {
            if(participants[i].number == number)
            {
                index = i;
                break;
            }
        }

        int delta = checkpoint - participants[index].lastCheckpoint;
        participants[index].lastCheckpoint = checkpoint;
        if (delta == 1 || Mathf.Abs(delta) > 2)
        {
            participants[index].points += 1;
        }

        UpdatePointText();
    }

    void UpdatePointText()
    {
        if(positionText == null) return;

        participants.Sort(SortByScore);
        string s = "";
        for(int i = participants.Count - 1; i >= 0; i--)
        {
            if(participants[i].number == playerNumber)
                s = s + "<b><color=\"#FF0000\">";
            s = s + participants[i].number.ToString() + ": " + participants[i].points.ToString() +"\n";
            if (participants[i].number == playerNumber)
                s = s + "</color></b>";
        }
        positionText.text = s;
    }

    static int SortByScore(Participant p1, Participant p2)
    {
        return p1.points.CompareTo(p2.points);
    }

    IEnumerator StartRace()
    {
        yield return new WaitForSeconds(1f);

        redLight.EnableKeyword("_EMISSION");
        yellowLight.DisableKeyword("_EMISSION");
        greenLight.DisableKeyword("_EMISSION");
        audioSource.PlayOneShot(horn);
        yield return new WaitForSeconds(1f);

        redLight.DisableKeyword("_EMISSION");
        yellowLight.EnableKeyword("_EMISSION");
        audioSource.PlayOneShot(horn);
        yield return new WaitForSeconds(1f);

        yellowLight.DisableKeyword("_EMISSION");
        greenLight.EnableKeyword("_EMISSION");
        audioSource.pitch = 1.2f;
        audioSource.PlayOneShot(horn);

        CarControl[] cars = FindObjectsOfType<CarControl>();

        for (int i = 0; i < cars.Length; i++)
        {
            cars[i].blockMovement = false;
        }
    }
}
