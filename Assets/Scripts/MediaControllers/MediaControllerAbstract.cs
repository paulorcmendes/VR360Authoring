﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// Author: Paulo Renato Conceição Mendes.<br/>
/// Basic controller for all media objects that are added to 360 videos.
/// </summary>
public abstract class MediaControllerAbstract: MonoBehaviour
{
    ///aux variable that contains the current index of the dictionary of positions
    private int current_index_pos;
    ///start time of the media object in seconds
    public float start_time;
    ///duaration time of the media object in seconds
    public float duration;
    ///volume of the media object
    public float volume;
    ///vertical angle of the media object in degrees
    private float theta;
    ///horizontal angle of the media object in degrees
    private float phi;
    ///media object distance from the center
    public float r;
    ///file path of the media object
    public string file_path;
    ///text of the media object, if any
    public string text;
    ///true if the media object executes in loop
    public bool loop;
    ///true if the media object follows the camera (fixed to the user)
    public bool follow_camera;
    ///main camera of the project
    public GameObject mainCamera;
    ///presentation360 game object of the scene
    public GameObject presentation360;
    ///vector that defines the origin from which the rotations are calculated
    private Vector3 origin;
    ///true if the mediaObject is playing
    private bool isPlaying;
    ///initial position of the media object
    private Vector3 start_pos;
    ///id of the current media object
    public string id;
    ///id the of media object that will be played when the current is selected
    public string on_select_name;
    ///id the of media object that will be played when the current is on focus
    public string on_focus_name;
    ///id the of media object that will be played when the current is out of focus
    public string during_out_of_focus_name;
    ///game object the of media object that will be played when the current is selected
    public GameObject on_select_object;
    ///game object the of media object that will be played when the current is on focus
    public GameObject on_focus_object;
    ///game object the of media object that will be played when the current is out of focus
    public GameObject during_out_of_focus_object;
    ///game object of the hotspot to which the current media object is pointed (in case of mirror)
    public GameObject src_hotspot_object;
    ///game object that is father of the current game object
    public GameObject father;
    ///initial time of the segment of the media that will be played
    public float clipBegin;
    ///final time of the segment of the media that will be played
    public float clipEnd;
    /// dictionary of the positions of the object over time
    public Dictionary<float, Vector3> timed_positions;
    /// pos frequency change
    public float pos_frequency;
    /// prefix used in subtitles
    public string prefix;
    /// <summary>
    /// Calls the play and invokes the stop of the media object (given its duration).
    /// </summary>
    public void InvokePlayStop()
    {
        //PlayMedia();
        SuperPlay();
        Invoke("SuperStop", duration);
    }
    /// <summary>
    /// Activates the colliders and sets that the media object is playing.
    /// </summary>
    public void SuperPlay()
    {
        this.isPlaying = true;
        if(GetComponent<Collider>()!=null) GetComponent<Collider>().enabled = true;
        this.PlayMedia();

        //Debug.Log(id+" played");
    }

    /// <summary>
    /// Deactivates the colliders and sets that the media object is not playing.
    /// </summary>
    public void SuperStop()
    {
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
        this.isPlaying = false;
        this.current_index_pos = 0;
        this.StopMedia();
        //Debug.Log(id + " stoppped");

    }
    /// <summary>
    /// This method is called when this media is selected.
    /// </summary>
    public void OnSelectMedia()
    {
        this.father.GetComponent<Video360Controller>().StopVideo360();
        this.on_select_object.SetActive(true);
        this.on_select_object.GetComponent<Video360Controller>().StartVideo360();
    }
    /// <summary>
    /// Returns wether the media object is playing.
    /// </summary>
    public bool IsPlaying
    {
        get
        {
            return this.isPlaying;
        }
    }
    /// <summary>
    /// Prepare the time that the media object will start given the start time.
    /// </summary>
    public void PrepareAnchors()
    {
        if (this.start_time > 0)
        {
            Invoke("InvokePlayStop", start_time);
        }else if(this.start_time == 0)
        {
            InvokePlayStop();
        }

        //changing positions over time
        foreach (float key in this.timed_positions.Keys)
        {
            Invoke("IterateOverPositions", key);
        }
    }
    /// <summary>
    /// Sets the initial configuration of the media object such as: its position and rotation, 
    /// wether it follows the camera, etc.
    /// </summary>
    /// <param name="id">id of the media object</param>
    /// <param name="father">father of the media object</param>
    /// <param name="start_time">start time of the media object in seconds</param>
    /// <param name="duration">duration of the media object in seconds</param>
    /// <param name="file_path">source file url of the media object</param>
    /// <param name="r">media object distance from the center </param>
    /// <param name="theta">vertical angle of the media object in degrees</param>
    /// <param name="phi">horizontal angle of the media object in degrees</param>
    /// <param name="volume">volume of the media object</param>
    /// <param name="loop">wether the media object executes in loop</param>
    /// <param name="follow_camera">wether the media object follows the camera (is fixed to the user)</param>
    /// <param name="text">text of the media object</param>
    /// <param name="on_select_name">media object that will be played when the current is selected</param>
    /// <param name="on_focus_name">media object that will be played when the current is on focus</param>
    /// <param name="during_out_of_focus_name">media object that will be played when the current is out of focus</param>
    /// <param name="clipBegin">initial time of the segment of the media that will be played</param>
    /// <param name="clipEnd">final time of the segment of the media that will be played</param>
    /// <param name="timedPositionsFile">file that contains the timed positions for the given object</param>
    /// <param name="prefix">prefix that comes before subtitles</param>
    public virtual void Configure(string id, GameObject father, float start_time, float duration, 
                                string file_path, float r, float theta, float phi, float volume, 
                                bool loop, bool follow_camera, string text, 
                                string on_select_name, string on_focus_name, string during_out_of_focus_name,
                                float clipBegin, float clipEnd, string timedPositionsFile, string prefix)
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        presentation360 = GameObject.FindGameObjectWithTag("Presentation360");
        this.id = id;
        this.start_time = start_time;
        this.duration = duration;
        this.volume = volume;
        this.file_path = file_path;
        this.loop = loop;
        this.follow_camera = follow_camera;
        this.text = text;
        this.theta = theta;
        this.phi = phi;
        this.r = r;
        this.isPlaying = false;
        this.on_select_name = on_select_name;
        this.on_focus_name = on_focus_name;
        this.during_out_of_focus_name = during_out_of_focus_name;
        this.father = father;
        this.clipBegin = clipBegin;
        this.clipEnd = clipEnd;
        this.prefix = prefix;
        this.origin = new Vector3(0, 0, r);
        this.start_pos = Utils.PolarToCartesian(this.origin, theta, phi);
        this.timed_positions = Utils.Vector3TimedPositions(this.start_pos,
            Utils.ReadTimedPositions(timedPositionsFile, out this.pos_frequency));        

        this.ChangePolarPos(new_pos: this.start_pos);
    }
    /// <summary>
    /// Auxiliary function to iterate over positions over time.
    /// </summary>
    private void IterateOverPositions()
    {
        float currentKey = this.timed_positions.Keys.ToList()[current_index_pos];
        ChangePolarPos(timed_positions[currentKey]);
        this.current_index_pos++;
    }
    /// <summary>
    /// Change the current position of the object
    /// </summary>
    /// <param name="new_pos">The new position of the object</param>
    private void ChangePolarPos(Vector3 new_pos)
    {
        if (this.follow_camera)
        {
            this.transform.parent = mainCamera.transform;
            this.transform.localPosition = new_pos;
            this.transform.LookAt(mainCamera.transform.position, mainCamera.transform.up);
        }
        else
        {
            this.transform.localPosition = new_pos;
            this.transform.LookAt(Vector3.zero, Vector3.up);
        }

        this.transform.Rotate(new Vector3(0, 180, 0));
    }
    /// <summary>
    /// Abort the media and the anchors that make it play, if any.
    /// </summary>
    public void AbortMedia()
    {
        SuperStop();
        StopMedia();
        CancelInvoke();
    }
    /// <summary>
    /// Abstract method that should be implemented by any media object type.
    ///  This method should configure the initial state of the components that are specific to the media object
    /// </summary>
    public abstract void Load();
    /// <summary>
    /// Abstract method that should be implemented by any media object type.
    ///  This method should configure how the components specific of the media object behave to make it stop
    /// </summary>
    public abstract void StopMedia();

    /// <summary>
    /// Abstract method that should be implemented by any media object type.
    ///  This method should configure how the components specific of the media object behave to make it play
    /// </summary>
    public abstract void PlayMedia();   

}