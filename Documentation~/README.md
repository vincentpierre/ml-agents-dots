# ML-Agents DOTS 

## Installation

### Install C# code
 * Create a new Project on Unity 2021.3.19f1
 * Navigate to the new created project folder and add the following entries into `Package/manifest.json` under "Dependencies":
 ```json
 "com.unity.ai.mlagents": "https://github.com/vincentpierre/ml-agents-dots.git",
 ```
 * In your Unity project, you should see the Package Manager resolving the new packages (this may take several minutes).
 * Go to Window -> Package Manager. Select DOTS ML-Agents and import all the samples.



### Install ML-Agents DOTS Python code
#### From Github on Windows
__(Note the escaped `&`)__
```
pip install git+https://github.com/vincentpierre/ml-agents-dots.git#egg=mlagents-dots-envs^&subdirectory=ml-agents-dots-envs~
pip install git+https://github.com/vincentpierre/ml-agents-dots.git#egg=mlagents-dots-learn^&subdirectory=ml-agents-dots-learn~
```
#### From Github on Linux and Max
```
pip install git+https://github.com/vincentpierre/ml-agents-dots.git#egg=mlagents-dots-envs&subdirectory=ml-agents-dots-envs~
pip install git+https://github.com/vincentpierre/ml-agents-dots.git#egg=mlagents-dots-learn&subdirectory=ml-agents-dots-learn~
```
#### From Source
 * Clone this repository in a new folder
 ```
 git clone https://github.com/vincentpierre/ml-agents-dots.git
 ```
 * Run the following commands inside the cloned repository in a virtual environment:
 ```
 pip install -e ./ml-agents-dots-envs~
 pip install -e ./ml-agents-dots-learn~
 ```


## Train an Environment
 * From the project window, open the Basic scene under `Assets\Samples\DOTS ML-Agents\0.X.0-preview\Basic\Scene\`
 * To start training, call
 ```
 mlagents-dots-learn
 ```
 and press play in the Editor.
 If the installation was successful, you should see in the Basic environment learn.


## API
A `Policy` holds data, processes it and the data can then be retrieved.
You can access the `Policy` in the main thread and use an inference model with it with the following code:

```csharp
var policy = new Policy(
  100,                              // The maximum number of agents that can request a decision per step
  new int3[] { SensorShape.Vector(3),
               SensorShape.Matrix(84, 84, 3) }, // The observation shapes 
  3,                                // The number of continuous actions
  new int[]{},                      // The discrete action branches (here none)
  );

policy.SubscribePolicyWithBarracudaModel(Name, Model, InferenceDevice);
```

The `Policy` can be accessed in a Unity Job in a parallel manner.  Here is an example of a job in which observation data is added to the policy :

```csharp
public struct UserCreatedSensingJob : IJobParallelFor
    {
        public NativeArray<Entity> entities;
        public Policy policy;

        public void Execute(int i)
        {
            policy.RequestDecision(entities[i])
                .SetReward(1.0f)
                .SetObservation(0, new float3(3.0f, 0, 0)); // observation index and then observation struct

        }
    }
```

The job can called using this code. Alternatively, you can use the `Entities.ForEach` API (see the 3DBall example) :

```csharp
protected override JobHandle OnUpdate(JobHandle inputDeps)
{
    var job = new UserCreatedSensingJob{
	    policy = myPolicy,
	    entities = ...,
	    sensors = ...,
	    reward = ...,
    }
    return job.Schedule(N_Agents, 64, inputDeps);
}
```

Note that this API can also be called outside of a job and used in the main thread. There is no reliance at all on IComponentData and ECS which means that we do not have to feed the data with blitable structs and could use NativeArrays as well. Here is an example on how to feed sensor data from a NativeSlice:

```csharp
var visObs = VisualObservationUtility.GetVisObs(camera, 84, 84, Allocator.TempJob);
// Here camera is a UnityEngine.Camera, 84 is the width of the image (and height). This will generate a Native Array with the TempJob Allocator.
policy.RequestDecision(entities[i])
  .SetObservationFromSlice(1, visObs.Slice());
```

In order to retrieve actions use a custom job :

```csharp
public struct UserCreatedActionEventJob : IActuatorJob
    {
        public void Execute(ActuatorEvent data)
        {
            var tmp = data.GetContinuousAction<float3>();
            Debug.Log(data.Entity.Index + "  " + tmp.x);
        }
    }
```
The ActuatorEvent data contains the Entity that requested the decision and a `GetContinuousAction` and `GetDiscreteAction` methods to retrieve the action data in the event.

Another way to retrieve actions is to use the `ActionHashMapUtils` as follows :

```csharp
// Create hash map from entities to actions (here for discrete actions)
m_DiscreteAction = new NativeHashMap<Entity, ActionStruct>(1, Allocator.Persistent);
// Update the hash map with the actions (This will remove the actions from the policy)
m_Policy.UpdateDiscreteActionHashMap<ActionStruct>(m_DiscreteAction);

// You can then query the actions :
ActionStruct action = new ActionStruct();
m_DiscreteAction.TryGetValue(m_Entity, out action);
```

Where `ActionStruct` is a user defined action struct. This struct must contain only floats or float structs in the continuous case and only integers (or enums derived from integers) in the discrete case. The HashMap becomes a mapping from Entities (the ones that requested decisions) and their actions.

## UI to create a Policy

We currently offer a `PolicySpecs` struct that has a custom inspector drawer (you can add it to a MonoBehaviour to edit the properties of your Policy and even add a neural network for its behavior).
To generate the Policy with the given settings call `PolicySpecs.GetPolicy()`.

## Communication Between C# and Python
In order to exchange data with Python, we use shared memory. Python will create a small file that contains information required for starting the communication. The path to the file will be randomly generated and passed by Python to the Unity Executable as command line argument. For in editor training, a default file will be used. Using shared memory allows for faster data exchange and will remove the need to serialize the data to an intermediate format.

__Note__ : The python code for communication is located in [ml-agents-dots-envs~](./../ml-agents-dots-envs~).
