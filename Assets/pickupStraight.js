var startposition;

var speed = 10;
var direction = 1;

function Start () {
}

function FixedUpdate(){
	//offset = offset + 0.5;
	//gameObject.renderer.material.SetTextureOffset("_MainTex",Vector2(0,-Time.time*direction*speed/10));
}


function OnCollisionStay(collision : Collision) {

    var rigidbody = collision.gameObject.GetComponent.<Rigidbody>();
    var ray = new Ray (rigidbody.position, Vector3(0,-1,0));
	var hit : RaycastHit;
	if (Physics.Raycast (ray, hit, 1)) {
		//Debug.DrawLine (ray.origin, hit.point);
		if(hit.collider==gameObject.GetComponent.<Collider>())
		{
		    var rigidbodyspeed = rigidbody.velocity.magnitude;
		    
		    // Adjust the direction of the velocity.
		    //rigidbody.velocity = (1*rigidbody.velocity.normalized - direction * transform.forward ).normalized * rigidbodyspeed;
		    
		    //rigidbody.velocity = Vector3(rigidbody.velocity.x,
		    
		    // If the velocity, period, is too slow, speed it up a bit.
		    if(direction*rigidbody.velocity.magnitude*Vector3.Dot(rigidbody.velocity.normalized,-transform.forward)*10< speed)
		    {
		   		rigidbody.velocity = rigidbody.velocity - direction* transform.forward/5; //Accelleration Rate!          //3*conveyorVelocity * 
		    }
		    else if(direction*rigidbody.velocity.magnitude*Vector3.Dot(rigidbody.velocity.normalized,-transform.forward)*10> speed)
		    {
		   		rigidbody.velocity = rigidbody.velocity+ direction* transform.forward/5; //rigidbody.velocity - direction* transform.up/5; //Deccelleration Rate!          //3*conveyorVelocity * 
		    }
		}
	}        
}
    
