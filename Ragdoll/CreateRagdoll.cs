using Godot;


[Tool]
public partial class CreateRagdoll : Skeleton3D
{
	[ExportToolButton("Generate Ragdoll")]
	public Callable GenerateButton => Callable.From(Generate);

	public Skeleton3D AnimationSkeleton;

	[Export] public float CapsuleRadius = 0.1f;

	private Node3D root;
	private Skeleton3D anim;

	private void Generate()
	{
		Clear();
		GenerateSkeleton();
		GenerateRoot();
		CreateBones(root);
		CreateJoints(root);
	}

	private void GenerateSkeleton()
	{
		Node3D parent = GetParent<Node3D>();

		// использую флаг, чтобы скрипт не переносился
		var anim = this.Duplicate(0);
		anim.Name = Name + "_anim";
		parent.AddChild(anim);
		anim.Owner = GetOwner<Node>();

		foreach (Node child in anim.GetChildren())
		{
			child.Free();
		}

		AnimationSkeleton = anim as Skeleton3D;
	}

	private void Clear()
	{
		Node3D parent = GetParent<Node3D>();

		foreach (Node child in GetChildren())
		{
			if (child.Name.ToString().Contains("Ragdoll"))
				child.Free();
		}

		foreach (Node child in parent.GetChildren())
		{
			if (child.Name.ToString().Contains("_anim"))
				child.Free();
		}
	}

	private void GenerateRoot()
	{
		root = new Node3D();
		root.Name = "Ragdoll";
		AddChild(root);
		root.Owner = GetOwner<Node>();
	}

	private void CreateBones(Node3D root)
	{
		for (int i = 0; i < GetBoneCount(); i++)
		{
			if (GetBoneChildren(i).Length == 0)
				continue;

			var bone = new RagdollBone();
			bone.Name = GetBoneName(i) + "_rag";
			bone.BoneName = GetBoneName(i);
			bone.PhysicsSkeleton = this;

			bone.GlobalTransform = GetBoneGlobalPose(i);

			var col = new CollisionShape3D();

			col.Shape = new CapsuleShape3D
			{
				Radius = CapsuleRadius,
				Height = CapsuleRadius * 2f
			};

			col.Name = GetBoneName(i) + "_col";


			// масса
			string name = bone.Name.ToString();

			if (name.Contains("Head"))
			{
				bone.Mass = 5.0f;
			}
			else if (name.Contains("Arm") || name.Contains("Forearm") || name.Contains("Hand"))
			{
				bone.Mass = 2.0f;
			}
			else if (name.Contains("Neck"))
			{
				bone.Mass = 3.0f;
			}
			else if (name.Contains("Leg") || name.Contains("Calf") || name.Contains("Foot"))
			{
				bone.Mass = 6.0f;
			}
			else // Torso / pelvis / spine
			{
				bone.Mass = 15.0f;
			}

			root.AddChild(bone);
			bone.AddChild(col);

			bone.Owner = GetOwner<Node>();
			col.Owner = GetOwner<Node>();
		}
	}

	private RagdollBone FindBone(Node node, string name)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is RagdollBone rb && rb.Name == name)
				return rb;

			var found = FindBone(child, name);
			if (found != null)
				return found;
		}
		return null;
	}


	// private void CreateJoints(Node3D root)
	// {
	// 	for (int i = 0; i < GetBoneCount(); i++)
	// 	{

	// 		int parentIndex = GetBoneParent(i);
	// 		if (parentIndex < 0) continue;

	// 		var parentBone = FindBone(root, GetBoneName(parentIndex) + "_rag");
	// 		var childBone = FindBone(root, GetBoneName(i) + "_rag");

	// 		if (parentBone == null || childBone == null) continue;

	// 		var joint = new RagdollJoint();
	// 		joint.Name = $"{parentBone.Name}_to_{childBone.Name}_joint".Replace("_rag", "");

	// 		parentBone.AddChild(joint);
	// 		joint.GlobalTransform = GetBoneGlobalPose(i);

	// 		joint.NodeA = joint.GetPathTo(parentBone);
	// 		joint.NodeB = joint.GetPathTo(childBone);

	// 		joint.BoneA = parentBone;
	// 		joint.BoneB = childBone;

	// 		joint.PhysicsSkeleton = this;
	// 		joint.AnimationSkeleton = AnimationSkeleton;

	// 		joint.Owner = GetOwner<Node>();
	// 	}


	// }
	private void CreateJoints(Node3D root)
	{
		for (int i = 0; i < GetBoneCount(); i++)
		{
			int parentIndex = GetBoneParent(i);
			if (parentIndex < 0) continue;

			var parentBone = FindBone(root, GetBoneName(parentIndex) + "_rag");
			var childBone = FindBone(root, GetBoneName(i) + "_rag");
			if (parentBone == null || childBone == null) continue;

			var joint = new RagdollJoint();
			joint.Name = $"{parentBone.Name}_to_{childBone.Name}_joint".Replace("_rag", "");

			parentBone.AddChild(joint);
			joint.GlobalTransform = GetBoneGlobalPose(i);

			joint.NodeA = joint.GetPathTo(parentBone);
			joint.NodeB = joint.GetPathTo(childBone);

			joint.BoneA = parentBone;
			joint.BoneB = childBone;
			joint.PhysicsSkeleton = this;
			joint.AnimationSkeleton = AnimationSkeleton;

			joint.Owner = GetOwner<Node>();

			SetupJointLimits(joint, childBone.Name);
		}
	}

	private void SetupJointLimits(RagdollJoint joint, string childName)
	{
		float degToRad = Mathf.DegToRad(1f);

		if (childName.Contains("Forearm") || childName.Contains("Calf"))
		{
			// hinge‑like: limit X axis only
			joint.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
			joint.SetParamX(Generic6DofJoint3D.Param.AngularLowerLimit, 0f);
			joint.SetParamX(Generic6DofJoint3D.Param.AngularUpperLimit, 130f * degToRad);

			joint.SetFlagY(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
			joint.SetFlagZ(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
		}
		else
		{
			float swing = childName.Contains("Head") ? 45f * degToRad : 30f * degToRad;
			float twist = childName.Contains("Head") ? 25f * degToRad : 15f * degToRad;

			joint.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
			joint.SetParamX(Generic6DofJoint3D.Param.AngularLowerLimit, -swing);
			joint.SetParamX(Generic6DofJoint3D.Param.AngularUpperLimit, swing);

			joint.SetFlagY(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
			joint.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, -swing);
			joint.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, swing);

			joint.SetFlagZ(Generic6DofJoint3D.Flag.EnableAngularLimit, true);
			joint.SetParamZ(Generic6DofJoint3D.Param.AngularLowerLimit, -twist);
			joint.SetParamZ(Generic6DofJoint3D.Param.AngularUpperLimit, twist);
		}
	}


}
