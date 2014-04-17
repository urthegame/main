using UnityEngine;
using System.Collections;

/**
 * Telpai piederoshs parametrs - kaada veida telpa taa ir
 */ 
public class RoomRoles {
    
    public int Selected = 0; //izveeleetaa telpas loma 

    private Room room; 
 
    public static string[] Names = {
        "Unassigned",//  0, 
        "Stairs" ,//  1, 
        "Kitchen" ,//  2, 
        "Bedroom" ,//  3, 
        "Bathroom" ,//  4, 
    };

    public static Color[] FloorColors = {
        new Color(1F, 1F, 1F, 1F) ,//  0, 
        new Color(0.5F, 0.5F, 0.5F, 1F) ,//  1, 
        new Color(0.2F, 0.1F, 0.69F, 1F) ,//  2, 
        new Color(0.75F, 0.13F, 0.1F, 1F) ,//  3, 
        new Color(0.1F, 0.691F, 0.1F, 1F) ,//  4, 
    };


    public RoomRoles(Room room){
        this.room = room;
    }



    public void SetRole(int newRole){
        //vai driikst mainiit ?  -- to skataas GUI skriptaa


        //ja newRole > 0 jaaizveido renovaacijas buuvdarbinsh
        //ja newRole ==0, tad jaaizveedo buuvdarbinsh, kas novaac visu saturu un noreseto telpu
        //ja jau notiek buuvdarbinsh, tad neljaut neko mainiit


        Selected = newRole;

        foreach(Transform child in room.transform.FindChild("geo").transform) {
            child.renderer.material.color = FloorColors[newRole];
        }

       // room.transform.FindChild("geo").transform.FindChild("floor").renderer.material.color = FloorColors[newRole];



    }



}