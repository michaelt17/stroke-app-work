using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Movement : MonoBehaviour {
	public float m_direction = -10f;
	public float y_direction = 0;

	public float speed = 9f;
	public float y_speed = 0;
	public float Curr_low_height = 1.3f;
	public float Curr_max_height = 2.65f;

	private bool isGrounded = true; // is the ball on the ground
	private bool isOverThreshold = false; // is the phone tilted enough to make the ball jump
    public bool isGyroscope = false;

	private Rigidbody2D m_rigidBody; 
	private Collision2D m_collision;
    private int frames = 0;
    private int units = 0;
	public int repCounter; 
	public Transform plank;
	public int plankCount = 0;  // number of planks currently made in the game
	public int planksJumped = 0; // number of planks ball has jumped over
	private int plankDirection = 1;
	private float heightOfHighestPlank = 0;
	private int heightDifferenceBetweenPlanks = 4;
    private float lowest_point = 0;
    private float highest_point = 0;
    private float[] y_values ;
    public List<float> value_ps = new List<float>(); //values per second
    public List<float> value_phs = new List<float>(); //values per half second
    public List<float> value_pqs = new List<float>(); //values per quarter second
    public List<float> value_p3qs = new List<float>(); //values per third quarter second
	public List<float> x_val_pqs = new List<float>();
	public float thresh_gyro =0;
    private float flex_threshold = .5f;
	private float thresholdVal = .5f;
    private Gyroscope gyro;
	private bool first_jump=true;

	public float x_start = 0;
	public float y_start = -3;
	public float curr_speed = 0;
	public float prev_speed = 0;

	public bool WentOver = false;
	public bool WentDown = false;



	// Use this for initialization

    // Use this for initialization
    void Start () {
		m_rigidBody = GetComponent<Rigidbody2D> ();

		Physics2D.gravity = Vector2.zero;

		repCounter = 0;

        //use y_values to store the angle.
        y_values = new float[20];
        //value_ps to store data per second
        if(SystemInfo.supportsGyroscope)
        {
			gyro = Input.gyro;
			gyro.enabled = true;
            isGyroscope = true;
			Debug.Log("Gyroscope Enable!");
        }
        else
        {
            Debug.Log("Accelerometer Enable!");
        }
        value_ps = new List<float>();
        value_phs = new List<float>();
        value_p3qs = new List<float>();
        value_pqs = new List<float>();
        InvokeRepeating("Recordingdata", 0.0f, 0.25f);

		// create the first 2 planks in the scene
		createPlank ();
		createPlank ();
	}



	// Update is called once per frame
	private void FixedUpdate () {
		if (GameManager.gameStarted) {

			

			m_rigidBody.velocity = new Vector2 (m_direction * -1, y_speed) * speed;

			Debug.Log (WentOver + " " + WentDown + " " + m_rigidBody.position.y + " " + Curr_max_height + " " + Curr_low_height);
		
			curr_speed = Input.acceleration.y;

			MoveHorizontally ();
			
				

            /* we no longer record the data per frame.
             * frames++;
            if(frames % 20 == 0)//Process y_values every 20frames.
            {
                Frame20Update();
            }
            y_values[frames % 20] = Input.acceleration.y;
            */



			// if you are passing a plank, create a new plank
			if (m_rigidBody.position.y > heightOfHighestPlank - heightDifferenceBetweenPlanks * 2) {
				planksJumped += 1;
				createPlank ();
			}



			if (curr_speed > prev_speed) {
				y_speed = -2 * (curr_speed - prev_speed);
			} else if (curr_speed < prev_speed) {
				y_speed = -1 * (curr_speed - prev_speed);
			} 

			//else {
			//y_speed = 0;}
			else if (Input.GetKeyDown (KeyCode.UpArrow)) {
				//GetComponent<Rigidbody2D> ().AddForce(new Vector2(0,1000));

				//m_rigidBody.velocity = new Vector2 (m_direction, 1) * speed;

				y_speed = 1;
			} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
				//m_rigidBody.velocity = new Vector2 (m_direction, -1) * speed;

				y_speed = -1;
			}

			if (m_rigidBody.position.y >= Curr_max_height && WentOver == false) 
			{
				WentOver = true;
			}
			if (WentOver && m_rigidBody.position.y >= Curr_max_height && y_speed >= 0) {
				y_speed = 0;
			}
			if (WentOver == true && m_rigidBody.position.y <= Curr_low_height)
			{
				WentDown = true;
			}
			if (WentDown && WentOver)
			{
				Curr_max_height += heightDifferenceBetweenPlanks;
				Curr_low_height += heightDifferenceBetweenPlanks;
				WentDown = false;
				WentOver = false;
				repCounter += 1;
			}
        }
        


	}

	void createPlank() {
		Instantiate(plank, new Vector2(2.29f*plankDirection, heightOfHighestPlank), Quaternion.identity); 
		plankCount++;
		plankDirection *= -1; //change the direction of where the plank is made (left or right)
		heightOfHighestPlank += heightDifferenceBetweenPlanks;
	}


    void Recordingdata()
        /* Basic time unit is .25 second. 
         * Based on this time interval, we record the data as
         * .25 second, .5 second, .75 second and 1 second.
         */
    {
        if (GameManager.gameStarted)
        {
			units++;
			float val_to_add;
			float val_to_add_x;
			//if there is the gyroscope put the value found with the gyroscope
			if (isGyroscope) {
				val_to_add = Input.gyro.rotationRateUnbiased.y;
				val_to_add_x = Input.gyro.rotationRateUnbiased.x;
				x_val_pqs.Add (val_to_add_x);
			} 
			//if there is not, use accelerometer data
			else {
				val_to_add = Input.acceleration.y;
			}

			value_pqs.Add(val_to_add);
            if (units % 2 == 0)//Process y_values every half second.
            {
				value_phs.Add(val_to_add);
                if (units % 4 == 0)//Process y_values every second.
                {
					value_ps.Add(val_to_add);
                }
            }
            if (units % 3 == 0)//Process y_values every three quarter second.
            {
				value_p3qs.Add(val_to_add);
            }
        }
    }


/*  debugging for transferring array value between scripts.
 *  public void Showvaluesize()
    {
        Debug.Log(value_ps[0]);
        Debug.Log(value_ps.Count);
    }

 */   
	/*Sets high or low value if it detects that there is a change in directions of movement.
	Approach is as follows:
		-Determining whether increasing or decreasing:
			-Finding the first order differences and if increasing, the overall motion must be up, opposite is true for opposite motion
				-If value is greater than a certain threshold value, we can confidently determine the behavior of the y_values
		-Determining whether there is a maximum value within decreasing values or a minimum value within increasing values:
			-Find second order difference
	Flawed Approach. See Set_flex_high_low
	private void high_low()
	{
		//
		float high_value=y_values[0];
		float low_value = y_values [0];
		float netChange = 0;
		float[] diffarray = new float[19];
		float diff_array_sum = 0;
		for (int i=1; i<19; i++) {
			float val = y_values [i];
			if (high_value < val) {
				high_value = val;
			}
			else if(low_value > val){
				low_value = val;
			}
			float prev_val = y_values [i - 1];
			float first_order_diff = val - prev_val;
			diffarray [i - 1] = first_order_diff;
			diff_array_sum += first_order_diff;
		}
		if(
	}*/


	/*Approach:
		-Reduce inaccuracies in the data, by averaging 4 data points in the 20 frames into a single average point for all 4 frames 
        (some will be higher than desired others will be lower, but on average they should be close to actual value)
		-From resulting array, see if it goes low high low -> max point, or high-low-high-> min point
	*/
/*	private void set_flex_high_low(){
		//reduces array
		float[] reduced_arr = new float[5];
		for (int i = 0; i < 19; i = i + 4) {
			reduced_arr [i / 4] = (y_values [i] + y_values [i + 1] + y_values [i + 2] + y_values [i + 3]) / 4;
		}
		for (int i = 1; i < reduced_arr.Length - 1; i++) {
			if (reduced_arr [i] > reduced_arr [i - 1] && reduced_arr [i] > reduced_arr [i + 1]) {
				highest_point = reduced_arr [i];
			}
			else if(reduced_arr [i] < reduced_arr [i - 1] && reduced_arr [i] < reduced_arr [i + 1]) {
				lowest_point = reduced_arr [i];
			}
		}
	}*/

	private void MoveHorizontally()
	{
		Vector2 movement;
		if (isGrounded) 
		{
			movement = new Vector2(m_direction,0);
		} 
		else // if ball is in the air, apply vertical velocity also
		{
			movement = new Vector2(m_direction,m_rigidBody.velocity.y);
		}


	}

	/*private void Gravity()
	{
		Vector2 movement;
		if (!isGrounded) 
		{
			movement = new Vector2 (Input.acceleration.x, -.98);
			//Input.acceleration = movement;
		}
		else
		{
			movement = new Vector2 (Input.acceleration.x, 0);
			//Input.acceleration = movement;
		}

	}*/

	// unity builtin function for dealing with collision, called every frame
	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "RightWall" || coll.gameObject.tag == "LeftWall")
			m_direction *= -1;
		if (coll.gameObject.tag == "Ground"){
			isGrounded = true;
		}

	}


/*void FixedUpdate()

{
if (GameManager.gameStarted) 
	MoveHorizontally ();
		// If there is a gyroscope use that
		if(isGyroscope)
		{
			Debug.Log("Using Gryo");
			var gyroSpeed_y= Input.gyro.rotationRateUnbiased.y;
			if (Mathf.Abs (gyroSpeed_y) > 4f) 
			{ 
				isOverThreshold = true;
				jump ();
			}

             we no longer record the data per frame.
             * frames++;
            if(frames % 20 == 0)//Process y_values every 20frames.
            {
                Frame20Update();
            }
            y_values[frames % 20] = Input.acceleration.y;
            
			

			if (m_rigidBody.position.y > heightOfHighestPlank - heightDifferenceBetweenPlanks * 2) 
			{
				planksJumped += 1;
				createPlank ();
			}

			if (isOverThreshold && Mathf.Abs (gyroSpeed_y) < 1f) 
			{

				repCounter += 1;
				isOverThreshold = false;
			}

			}
		
		//else Use Accerolometer
		Debug.Log("Using Accelerometer");
		if (Mathf.Abs (Input.acceleration.y) > thresholdVal)


		{
			isOverThreshold = true;
			jump ();
		}
		// if you are passing a plank, create a new plank
		if (m_rigidBody.position.y > heightOfHighestPlank - heightDifferenceBetweenPlanks * 2) 
		{
			planksJumped += 1;
			createPlank ();
		}

		// if your arm is lowered back down
		if (isOverThreshold && Mathf.Abs (Input.acceleration.y) < .5f) 
		{

			repCounter += 1;
			isOverThreshold = false;
		}

		// press space to jump if played on unity player
		if (Input.GetKeyDown (KeyCode.Space)) 
		{
			jump ();
		}

            
}*/
			}