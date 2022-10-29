﻿namespace Starboard.Resources

open Starboard
open Starboard.Resources

type Pod = { 
    metadata: Metadata
    containers: Container list
    volumes: Volume list
}

// https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/pod-v1/
type Pod with
    static member empty =
        { 
            metadata = Metadata.empty
            containers = List.empty 
            volumes = List.empty
        }

// resource: version, kind, metadata, spec
// template: metadata, spec
    member this.K8sVersion() = "v1"
    member this.K8sKind() = "Pod"
    member this.K8sMetadata() = Metadata.ToK8sModel this.metadata
    member this.Spec() =
        {|
            // https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/pod-v1/#PodSpec
            containers = this.containers |> Helpers.mapEach (fun c -> c.Spec())
            volumes = this.volumes |> Helpers.mapEach (fun v -> v.Spec())
        |}
    member this.ToResource() =
        {|
            apiVersion = this.K8sVersion()
            kind = this.K8sKind()
            metadata = this.K8sMetadata()
            spec = this.Spec()
        |}
        

type PodBuilder() =
        
    member _.Yield (_) = Pod.empty

    member __.Zero () = Pod.empty
    
    member __.Combine (currentValueFromYield: Pod, accumulatorFromDelay: Pod) = 
        { currentValueFromYield with 
            metadata = Metadata.combine currentValueFromYield.metadata accumulatorFromDelay.metadata
            containers = List.append (currentValueFromYield.containers) (accumulatorFromDelay.containers)
            volumes = List.append (currentValueFromYield.volumes) (accumulatorFromDelay.volumes)
        }
    
    member __.Delay f = f()
    
    member this.For(state: Pod , f: unit -> Pod) =
        let delayed = f()
        this.Combine(state, delayed)
    

    /// Name of the Pod. 
    /// Name must be unique within a namespace. 
    /// https://kubernetes.io/docs/reference/kubernetes-api/common-definitions/object-meta/#ObjectMeta
    [<CustomOperation "name">]
    member _.Name(state: Pod, name: string) = 
        let newMetadata = { state.metadata with name = name }
        { state with metadata = newMetadata }

    /// Namespace of the Pod.
    /// Namespace defines the space within which each name must be unique. Default is "default".
    /// https://kubernetes.io/docs/reference/kubernetes-api/common-definitions/object-meta/#ObjectMeta
    [<CustomOperation "ns">]
    member _.Namespace(state: Pod, ns: string) = 
        let newMetadata = { state.metadata with ns = ns }
        { state with metadata = newMetadata }
        
    /// Labels for the Pod
    [<CustomOperation "labels">]
    member _.Labels(state: Pod, labels: (string*string) list) = 
        let newMetadata = { state.metadata with labels = labels }
        { state with metadata = newMetadata }

    /// Annotations for the Pod
    [<CustomOperation "annotations">]
    member _.Annotations(state: Pod, annotations: (string*string) list) = 
        let newMetadata = { state.metadata with annotations = annotations }
        { state with metadata = newMetadata }

    // Container
    member this.Yield(container: Container) = this.Container(Pod.empty, container)
    member this.Yield(container: Container seq) = container |> Seq.fold (fun state x -> this.Container(state, x)) Pod.empty
    member this.YieldFrom(container: Container seq) = this.Yield(container)    
    [<CustomOperation "container">]
    member _.Container(state: Pod, container: Container) = { state with containers = List.append state.containers [container] }
        
    [<CustomOperation "containers">]
    member _.Containers(state: Pod, containers: Container list) = { state with containers = containers }

    // Volume
    member this.Yield(volume: Volume) = this.Volume(Pod.empty, volume)
    member this.Yield(volume: Volume seq) = volume |> Seq.fold (fun state x -> this.Volume(state, x)) Pod.empty
    member this.YieldFrom(volume: Volume seq) = this.Yield(volume)
    [<CustomOperation "volume">]
    member _.Volume(state: Pod, volume: Volume) = { state with volumes = List.append state.volumes [volume] }



[<AutoOpen>]
module PodBuilders =
    let pod = new PodBuilder()