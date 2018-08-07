using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour {
    public GameObject[] room_types;
    public GameObject start_room;

    protected int[] angles = new int[4];

    public int[] exit_data;

    public List<Vector3> room_centers;
    public List<Vector3> new_rooms;

    // Use this for initialization
    void Start() {
        angles[0] = 0;
        angles[1] = 90;
        angles[2] = 180;
        angles[3] = 270;

        room_centers = new List<Vector3>();
        new_rooms = new List<Vector3>();

        build_room_data();
        place_start_room();
    }

    void build_room_data() {
        room_types = Resources.LoadAll<GameObject>("Prefabs/Rooms");
        exit_data = new int[room_types.Length];
        for(int i = 0; i < room_types.Length; i++) {
            GameObject instance = Instantiate(room_types[i], new Vector3(-100f, -100f, 0f), Quaternion.identity);
            int exits = count_exits(new Vector3(-100f, -100f, 0));
            if(exits < 1 || exits > 4) {
                Debug.Log("Non-standard room located at room_types index " + i);
            } else {
                exit_data[i] = exits;
            }
            Destroy(instance);
        }
        
    }
	
	// Update is called once per frame
	void Update () {
        if(new_rooms.Count > 0) {
            //Debug.Log("New room center #" + (i + 1) + ": " + new_rooms[i]);
            check_room(new_rooms[0]);
            /*
            if(room_centers.Count > 15) {
                break;
            }
            */
        }
    }

    protected int count_exits(Vector3 centerpoint) {
        int num_exits= 0;
        //north
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(0, 2, 0))) {
            num_exits += 1;
        }
        //east
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(2, 0, 0))) {
            num_exits += 1;
        }
        //south
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(0, -2, 0))) {
            num_exits += 1;
        }
        //west
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(-2, 0, 0))) {
            num_exits += 1;
        }
        return num_exits;
    }

    void check_room(Vector3 centerpoint) {
        if(room_centers.Contains(centerpoint)) {
            if(new_rooms.Contains(centerpoint)) {
                Debug.Log("Room already exits, removing");
                new_rooms.Remove(centerpoint);
                new_rooms.TrimExcess();
            }
            return;
        }
        
        List<Vector3> exits = new List<Vector3>();

        int[] openings = new int[4];    //Values: 0 no opening, 1 possible opening, 2 required opening

        //Debug.Log("Centerpoint: " + centerpoint);

        //north
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(0, 3))) {
            exits.Add(centerpoint + new Vector3(0, 2));
            openings[0] = 1;
            Vector3 possible_room1 = centerpoint + new Vector3(0, 5);
            if(room_centers.Contains(possible_room1)){
                openings[0] = 2;
            }
        }
        //east
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(3, 0))) {
            exits.Add(centerpoint + new Vector3(2, 0));
            openings[1] = 1;
            Vector3 possible_room2 = centerpoint + new Vector3(5, 0);
            if(room_centers.Contains(possible_room2)) {
                openings[1] = 2;
            }
        }
        //south
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(0, -3))) {
            exits.Add(centerpoint + new Vector3(0, -2));
            openings[2] = 1;
            Vector3 possible_room3 = centerpoint + new Vector3(0, -5);
            if(room_centers.Contains(possible_room3)) {
                openings[2] = 2;
            }
        }
        //west
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(-3, 0))) {
            exits.Add(centerpoint + new Vector3(-2, 0));
            openings[3] = 1;
            Vector3 possible_room4 = centerpoint + new Vector3(-5, 0);
            if(room_centers.Contains(possible_room4)) {
                openings[3] = 2;
            }
        }
        
        int min_exits = 0;
        int max_exits = 4;
        
        for(int i = 0; i < openings.Length; i++) {
            
            if(openings[i] == 0) {
                max_exits--;
            }
            if(openings[i] == 2) {
                min_exits++;
            }
        }
        
        place_room(centerpoint, min_exits, max_exits, openings);
    }

    void place_room(Vector3 centerpoint, int min_exits, int max_exits, int[] openings) {

        //Debug.Log("min_exits: " + min_exits);
        //Debug.Log("max_exits: " + max_exits);

        //int exit_num = (int)Random.Range(min_exits, max_exits + 1);

        //Debug.Log("exit_num: " + exit_num);

        List<GameObject> valid_rooms = new List<GameObject>();
        for(int i = 0; i < exit_data.Length; i++) {
            if(exit_data[i] >= min_exits && exit_data[i] <= max_exits) {
                //Debug.Log("Adding valid room");
                valid_rooms.Add(room_types[i]);
            }
        }
        //Debug.Log("valid_rooms_list count: " + valid_rooms.Count);

        bool room_fit = false;
        while(room_fit == false){
            if(valid_rooms.Count == 0) {
                Debug.Log("Failed to place room at " + centerpoint);
                break;
            }
            int rand_index = (int)Random.Range(0, valid_rooms.Count);
            GameObject instance = Instantiate(valid_rooms[rand_index], centerpoint, Quaternion.identity);
            room_fit = rotate_to_fit(instance, openings);
            if(!room_fit) {
                valid_rooms.Remove(instance);
                Destroy(instance);
            }
        }

        if(!room_fit) {
            Debug.Log("place_room failed to fit rotation");
            new_rooms.Clear();
            return;
        }

        while(new_rooms.Contains(centerpoint)) {
            new_rooms.Remove(centerpoint);
        }
        
        new_rooms.TrimExcess();
        room_centers.Add(centerpoint);

        check_neighboors(centerpoint);
    }

    void check_neighboors(Vector3 centerpoint) {
        //north
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(0, 2))) {
            Vector3 possible_room1 = centerpoint + new Vector3(0, 5);
            if(!room_centers.Contains(possible_room1) && !new_rooms.Contains(possible_room1)) {
                new_rooms.Add(possible_room1);
                Debug.Log(centerpoint + " added new room at " + possible_room1 + " (north)");
            }
        }
        //east
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(2, 0))) {
            Vector3 possible_room2 = centerpoint + new Vector3(5, 0);
            if(!room_centers.Contains(possible_room2) && !new_rooms.Contains(possible_room2)) {
                new_rooms.Add(possible_room2);
                Debug.Log(centerpoint + " added new room at " + possible_room2 + " (east)");
            }
        }
        //south
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(0, -2))) {
            Vector3 possible_room3 = centerpoint + new Vector3(0, -5);
            if(!room_centers.Contains(possible_room3) && !new_rooms.Contains(possible_room3)) {
                new_rooms.Add(possible_room3);
                Debug.Log(centerpoint + " added new room at " + possible_room3 + " (south)");
            }
        }
        //west
        if(!Physics2D.OverlapPoint(centerpoint + new Vector3(-2, 0))) {
            Vector3 possible_room4 = centerpoint + new Vector3(-5, 0);
            if(!room_centers.Contains(possible_room4) && !new_rooms.Contains(possible_room4)) {
                new_rooms.Add(possible_room4);
                Debug.Log(centerpoint + " added new room at " + possible_room4 + " (west)");
            }
        }
    }

    protected bool rotate_to_fit(GameObject instance, int[] openings) {
        bool does_fit = true;
        List<int> angle_fit = new List<int>();

        Vector3 centerpoint = instance.transform.position;
        Vector3 north_point = centerpoint + new Vector3(0, 2);
        Vector3 east_point = centerpoint + new Vector3(2, 0);
        Vector3 south_point = centerpoint + new Vector3(0, -2);
        Vector3 west_point = centerpoint + new Vector3(-2, 0);

        for(int i = 0; i < angles.Length; i++) {
            does_fit = true;
            instance.transform.Rotate(0, 0, angles[i]);
            //north
            if(Physics2D.OverlapPoint(north_point)) {
                if(openings[0] == 2) {
                    does_fit = false;
                }
            }
            //east
            if(Physics2D.OverlapPoint(east_point)) {
                if(openings[1] == 2) {
                    does_fit = false;
                }
            }
            //south
            if(Physics2D.OverlapPoint(south_point)) {
                if(openings[2] == 2) {
                    does_fit = false;
                }
            }
            //west
            if(Physics2D.OverlapPoint(west_point)) {
                if(openings[3] == 2) {
                    does_fit = false;
                }
            }
            if(does_fit) {
                angle_fit.Add(i);
            }
            instance.transform.rotation = Quaternion.identity;
        }
        if(angle_fit.Count > 0) {
            int rand_rotate = (int)Random.Range(0, angle_fit.Count);
            int new_rotate = angle_fit[rand_rotate];
            instance.transform.Rotate(0, 0, angles[new_rotate]);
            return true;
        }
        return false;
    }

    void place_start_room() {
        int new_angle = angles[(int)Random.Range(0, angles.Length)];
        GameObject instance = Instantiate(start_room, new Vector3(0f, 0f, 0f), Quaternion.identity);
        instance.transform.Rotate(0, 0, new_angle);

        Vector3 centerpoint = instance.transform.position;
        room_centers.Add(centerpoint);

        check_neighboors(centerpoint);
    }
}
