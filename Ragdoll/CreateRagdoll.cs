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
		// CreateJoints(root);
	}



	private void GenerateSkeleton()
	{
		Node3D parent = GetParent<Node3D>();


		Skeleton3D anim = this.Duplicate(0) as Skeleton3D;
		if (anim == null) return; // на всякий случай
		anim.Name = Name + "_anim";

		foreach (Node child in anim.GetChildren())
		{
			child.Free();
		}

		parent.AddChild(anim);

		anim.Owner = GetOwner<Node>();


		AnimationSkeleton = anim;
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

		// root.Transform = this.Transform;
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
			bone.PhysicsSkeleton = this;

			bone.BoneName = GetBoneName(i);

			bone.GlobalTransform = GetBoneGlobalPose(i);
			var col = new CollisionShape3D();
			col.Shape = new CapsuleShape3D
			{
				Radius = CapsuleRadius,
				Height = CapsuleRadius * 2f
			};
			col.Name = GetBoneName(i) + "_col";


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

			joint.NodeA = joint.GetPathTo(parentBone);
			joint.NodeB = joint.GetPathTo(childBone);
			joint.AnimationSkeleton = AnimationSkeleton;
			joint.BoneA = parentBone;
			joint.BoneB = childBone;
			joint.PhysicsSkeleton = this;
			joint.Owner = GetOwner<Node>();
		}
	}

}
