
var speed = 10;

var turnright:int = 1;
var offset:int;
var startangle:float = 0;
var destroybags:boolean = false;

function Start () {
	//speed=speed/10;
}

function Update () {

}

function OnCollisionStay(collision : Collision) {

	if(destroybags)
	{
		Destroy(collision.gameObject);	
	}
	//return;
	var rigidbody = collision.gameObject.GetComponent.<Rigidbody>();
	var relativeposition = gameObject.transform.InverseTransformPoint(rigidbody.transform.position);

    if(relativeposition.z>0&&relativeposition.x<0){
	    var rigidbodyspeed = rigidbody.velocity.magnitude;
	    
	    
	    var distancetocenter = (rigidbody.transform.position - gameObject.transform.position);
	    
	    var xvec= new Vector3(0,0,0);
	    var yvec= new Vector3(0,0,0);
	    
	    //rigidbody.transform.position.
	    if(turnright==1){
		    xvec = new Vector3(distancetocenter.z,0,0);
		    yvec = new Vector3(0,0, -distancetocenter.x);
		    //rigidbody.transform.rotation = Quaternion.AngleAxis(rigidbody.transform.rotation.eulerAngles.y + Time.deltaTime*speed*1,Vector3(0,1,0));    
		 
	    }else if(turnright==0){
		    xvec = new Vector3(-distancetocenter.z,0,0);
	    	yvec = new Vector3(0,0, distancetocenter.x);
	    	//rigidbody.transform.rotation = Quaternion.AngleAxis(rigidbody.transform.rotation.eulerAngles.y - Time.deltaTime*speed*1,Vector3(0,1,0));	
		}
	    // unit vector i want to be going.
	    var directionitshouldbe = ( xvec+ yvec).normalized;
	    
	        
	    
	    // adjusts the velocity direction to what it should be.
	    //rigidbody.velocity = (1*rigidbody.velocity.normalized + directionitshouldbe).normalized *rigidbodyspeed;
	    //rigidbody.transform.Rotate(Vector3(0,1,0)*.02);
	    
	
	    
	    // If the velocity, period, is too slow, speed it up a bit.
	   // if(rigidbody.velocity.magnitude*10< speed){
	   //		rigidbody.velocity = rigidbody.velocity + directionitshouldbe/5; //Accelleration Rate!          //3*conveyorVelocity * 
	   // }
	   // else if(rigidbody.velocity.magnitude*10> speed){
	   //		rigidbody.velocity = directionitshouldbe*speed/10;//rigidbody.velocity - directionitshouldbe/5; //Deccelleration Rate!          //3*conveyorVelocity * 
	   // }
	    
        if(rigidbody.velocity.magnitude*Vector3.Dot(rigidbody.velocity.normalized,directionitshouldbe)*10< speed)
	    {
	   		rigidbody.velocity = rigidbody.velocity + directionitshouldbe/5; //Accelleration Rate!          //3*conveyorVelocity * 
	    }
	    else if(rigidbody.velocity.magnitude*Vector3.Dot(rigidbody.velocity.normalized,directionitshouldbe)*10> speed)
	    {
	   		rigidbody.velocity = rigidbody.velocity- directionitshouldbe/5; //rigidbody.velocity - direction* transform.up/5; //Deccelleration Rate!          //3*conveyorVelocity * 
	    }
		
	}
}