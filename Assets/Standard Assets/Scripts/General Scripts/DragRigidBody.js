var spring = 1000.0;
var damper = 1.0;
var drag = 1000.0;
var angularDrag = 5.0;
var distance = 0.0;
var attachToCenterOfMass = false;

private var springJoint : SpringJoint;    
private var lastbag: Rigidbody;
    
function Update ()
{
	// Make sure the user pressed the mouse down
	if (!Input.GetMouseButtonDown (0))
		return;

	var mainCamera = FindCamera();
		
	// We need to actually hit an object
	var hit : RaycastHit;
	if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition),  hit, 100))
		return;
	// We need to hit a rigidbody that is not kinematic
	if (!hit.rigidbody || hit.rigidbody.isKinematic)
		return;
	
	lastbag = hit.rigidbody;
	
	if (!springJoint)
	{
		var go = new GameObject("Rigidbody dragger");
		var body : Rigidbody = go.AddComponent.<Rigidbody>() as Rigidbody;
		springJoint = go.AddComponent.<SpringJoint>();
		body.isKinematic = true;
	}
	
	
	springJoint.transform.position = hit.point;
	if (attachToCenterOfMass)
	{
		var anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
		anchor = springJoint.transform.InverseTransformPoint(anchor);
		springJoint.anchor = anchor;
	}
	else
	{
		springJoint.anchor = Vector3.zero;
	}
	
	springJoint.spring = spring;
	springJoint.damper = damper;
	springJoint.maxDistance = distance;
	springJoint.connectedBody = hit.rigidbody;
	
	lastbag.freezeRotation = true;
	
	StartCoroutine ("DragObject", hit.distance);
}

function DragObject (distance : float)
{
	var oldDrag = springJoint.connectedBody.drag;
	var oldAngularDrag = springJoint.connectedBody.angularDrag;
	springJoint.connectedBody.drag = drag;
	springJoint.connectedBody.angularDrag = angularDrag;
	var mainCamera = FindCamera();
	while (Input.GetMouseButton (0))
	{
		var ray = mainCamera.ScreenPointToRay (Input.mousePosition);
		springJoint.transform.position = ray.GetPoint(distance);
		lastbag.velocity = new Vector3(10f,0.0f,1f);
		yield;
	} 
	lastbag.freezeRotation = false;
	if (springJoint.connectedBody)
	{
		springJoint.connectedBody.drag = oldDrag;
		springJoint.connectedBody.angularDrag = oldAngularDrag;
		springJoint.connectedBody = null;
	}
}

function FindCamera ()
{
	if (GetComponent.<Camera>())
		return GetComponent.<Camera>();
	else
		return Camera.main;
}