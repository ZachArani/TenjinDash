﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class JoyconManager : MonoBehaviour
{
    // Settings accessible via Unity
    public bool EnableIMU = true;
    public bool EnableLocalize = true;

    // Different operating systems either do or don't like the trailing zero
    private const ushort vendor_id = 0x57e;
    private const ushort vendor_id_ = 0x057e;
    private const ushort product_l = 0x2006;
    private const ushort product_r = 0x2007;

    public List<Joycon> j; // Array of all connected Joy-Cons
    static JoyconManager instance;
    public List<GameObject> players;

    public static JoyconManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        //if (instance != null) Destroy(gameObject);
        instance = this;
        int i = 0;

        j = new List<Joycon>();
        bool isLeft = false;
        HIDapi.hid_init();

        IntPtr ptr = HIDapi.hid_enumerate(vendor_id, product_l);
        IntPtr top_ptr = ptr;

        if (ptr == IntPtr.Zero)
        {
            ptr = HIDapi.hid_enumerate(vendor_id_, product_l);
            if (ptr == IntPtr.Zero)
            {
                HIDapi.hid_free_enumeration(ptr);
                Debug.Log("No Left Joy-Cons found!");
            }
        }
        hid_device_info enumerate;
        while (ptr != IntPtr.Zero)
        {
            enumerate = (hid_device_info)Marshal.PtrToStructure(ptr, typeof(hid_device_info));

            //Debug.Log (enumerate.product_id);
            isLeft = true;
            Debug.Log("Left Joy-Con connected.");
            IntPtr handle = HIDapi.hid_open_path(enumerate.path);
            HIDapi.hid_set_nonblocking(handle, 1);
            j.Add(new Joycon(handle, EnableIMU, EnableLocalize & EnableIMU, 0.05f, isLeft));
            ++i;
            ptr = enumerate.next;
        }

        ptr = HIDapi.hid_enumerate(vendor_id, product_r);
        top_ptr = ptr;

        if (ptr == IntPtr.Zero)
        {
            ptr = HIDapi.hid_enumerate(vendor_id_, product_r);
            if (ptr == IntPtr.Zero)
            {
                HIDapi.hid_free_enumeration(ptr);
                Debug.Log("No Right Joy-Cons found!");
            }
        }
        while (ptr != IntPtr.Zero)
        {
            enumerate = (hid_device_info)Marshal.PtrToStructure(ptr, typeof(hid_device_info));

            Debug.Log(enumerate.product_id);
            isLeft = false;
            Debug.Log("Right Joy-Con connected.");
            IntPtr handle = HIDapi.hid_open_path(enumerate.path);
            HIDapi.hid_set_nonblocking(handle, 1);
            j.Add(new Joycon(handle, EnableIMU, EnableLocalize & EnableIMU, 0.05f, isLeft));
            ++i;
            ptr = enumerate.next;
        }

        HIDapi.hid_free_enumeration(top_ptr);
    }

    void Start()
    {
        for (int i = 0; i < j.Count; ++i)
        {
            Joycon jc = j[i];
            byte LEDs = 0x0;
            LEDs |= (byte)(0x1 << i);
            jc.Attach(leds_: LEDs);
            jc.Begin();
        }
    }

    void Update()
    {
        for (int i = 0; i < j.Count; ++i)
        {
            j[i].Update();
        }
    }

    void OnApplicationQuit()
    {
        if (j != null && j.Count == 0)
        {
            return;
        }

        for (int i = 0; i < j.Count; ++i)
        {
            j[i].Detach();
        }
    }

    public Joycon GetJoyconByPlayer(GameObject player)
    {
        if (players.IndexOf(player) < j.Count)
        {
            return j[players.IndexOf(player)];
        }

        return null;
    }
}
