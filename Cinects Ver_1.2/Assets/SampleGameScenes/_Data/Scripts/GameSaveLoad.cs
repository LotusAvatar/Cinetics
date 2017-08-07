using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

public class GameSaveLoad : MonoBehaviour
{

    // An example where the encoding can be found is at 
    // http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
    // We will just use the KISS method and cheat a little and use 
    // the examples from the web page since they are fully described 

    // This is our local private members 
    //Rect _Save, _Load, _SaveMSG, _LoadMSG;
    bool _ShouldSave, _ShouldLoad, _SwitchSave, _SwitchLoad;
    string _FileLocation, _FileName;
    UserData myData;
    string _PlayerName;
    string _data;

    Vector3 VPosition;
    Vector3 IboxLeftBotBack;
    Vector3 IboxRightTopFront;

    private static GameSaveLoad instance;

    public static GameSaveLoad Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (GameSaveLoad)GameObject.FindObjectOfType(typeof(GameSaveLoad));
            }
            return instance;
        }
    }

    // When the EGO is instansiated the Start will trigger 
    // so we setup our initial values for our local members 
    void Start()
    {
        // Where we want to save and load to and from 
        _FileLocation = Application.dataPath;
        _FileName = "SaveData.xml";

        // for now, lets just set the name to Bruno Batista
        _PlayerName = "Bruno Batista";

        // we need soemthing to store the information into 
        myData = new UserData();
    }

    public void SavePlayerRange(Vector3 _IboxLeftBotBack, Vector3 _IboxRightTopFront, bool usingRightHand)
    {
        myData.rangeData.usingRightHand = usingRightHand;
        myData.rangeData.left = _IboxLeftBotBack.x;
        myData.rangeData.bot = _IboxLeftBotBack.y;
        myData.rangeData.back = _IboxLeftBotBack.z;
        myData.rangeData.right = _IboxRightTopFront.x;
        myData.rangeData.top = _IboxRightTopFront.y;
        myData.rangeData.fron = _IboxRightTopFront.z;

        _data = SerializeObject(myData);
        CreateXML();
        Debug.Log(_data);
    }

    public void LoadPlayerRange(ref Vector3 _IboxLeftBotBack, ref Vector3 _IboxRightTopFront, ref bool usingRightHand)
    {
        LoadXML();
        if (_data.ToString() != "")
        {
            // notice how I use a reference to type (UserData) here, you need this 
            // so that the returned object is converted into the correct type 
            myData = (UserData)DeserializeObject(_data);
            // set the players position to the data we loaded 
            IboxLeftBotBack = new Vector3(myData.rangeData.left, myData.rangeData.bot, myData.rangeData.back);
            _IboxLeftBotBack = IboxLeftBotBack;
            IboxRightTopFront = new Vector3(myData.rangeData.right, myData.rangeData.top, myData.rangeData.fron);
            _IboxRightTopFront = IboxRightTopFront;
            usingRightHand = myData.rangeData.usingRightHand;
            // just a way to show that we loaded in ok 
            Debug.Log(" *** Range Loaded! ***");
        }
    }

    /* The following metods came from the referenced URL */
    string UTF8ByteArrayToString(byte[] characters)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        string constructedString = encoding.GetString(characters);
        return (constructedString);
    }

    byte[] StringToUTF8ByteArray(string pXmlString)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] byteArray = encoding.GetBytes(pXmlString);
        return byteArray;
    }

    // Here we serialize our UserData object of myData 
    string SerializeObject(object pObject)
    {
        string XmlizedString = null;
        MemoryStream memoryStream = new MemoryStream();
        XmlSerializer xs = new XmlSerializer(typeof(UserData));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        xs.Serialize(xmlTextWriter, pObject);
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
        XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
        return XmlizedString;
    }

    // Here we deserialize it back into its original form 
    object DeserializeObject(string pXmlizedString)
    {
        XmlSerializer xs = new XmlSerializer(typeof(UserData));
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        return xs.Deserialize(memoryStream);
    }

    // Finally our save and load methods for the file itself 
    void CreateXML()
    {
        StreamWriter writer;
        FileInfo t = new FileInfo(_FileLocation + "\\" + _FileName);
        if (!t.Exists)
        {
            writer = t.CreateText();
        }
        else
        {
            t.Delete();
            writer = t.CreateText();
        }
        writer.Write(_data);
        writer.Close();
        Debug.Log("File written.");
    }

    void LoadXML()
    {
        StreamReader r = File.OpenText(_FileLocation + "\\" + _FileName);
        string _info = r.ReadToEnd();
        r.Close();
        _data = _info;
        Debug.Log("File Read");
    }
}

// UserData is our custom class that holds our defined objects we want to store in XML format 
public class UserData
{
    // We have to define a default instance of the structure 
    public Data _iUser;
    public RangeData rangeData;
    // Default constructor doesn't really do anything at the moment 
    public UserData() { }

    // Anything we want to store in the XML file, we define it here 
    public struct Data
    {
        public float x;
        public float y;
        public float z;
        public string name;
        public string professional;
    }

    public struct RangeData
    {
        public bool usingRightHand;
        public float left;
        public float bot;
        public float back;
        public float right;
        public float top;
        public float fron;
    }
}
//IboxLeftBotBack, ref IboxRightTopFront