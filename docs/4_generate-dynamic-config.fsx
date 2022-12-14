(**
---
title: Generate dynamic config
category: How-to
categoryindex: 2
index: 4
---
*)

(**
# Generate dynamic config

The real power of Overboard is not only the awesome declarative builder syntax enabled by F# but also that we can leverage the full power of a programming language instead of some hacky templating language. 

In the example below we define a function that defines a **deployment** and a **service** based on the application _name_ and _port_ number passed to the function. 

The function nomalizes the name by lowercasing it and stripping any special characters from it. This is a great way to standardize on naming conventions without always requiring that everyone always remembers the standards.
*)
(***hide***)
let makeYaml text = $"""<pre class="fssnip highlighted"><code lang="yaml">{text}</code></pre>"""


(*** hide ***)
#r "nuget:Overboard"

(*** show ***)
open System
open Overboard
open Overboard.Common
open Overboard.Workload
open Overboard.Service

/// Returns a deployment and service for `appName`
let publicApi appName portNumber =
    /// Lowercase the name and allow only letters and digits
    let normalize (name: string) =
        name.ToLowerInvariant().ToCharArray() 
        |> Array.filter Char.IsLetterOrDigit
        |> String
    let normalizedName = normalize appName
    let matchOn = ("app", normalizedName)
    k8s {
        deployment {
            $"{normalizedName}-deploy"
            _labels [matchOn]
            replicas 2
            add_matchLabel ("app", normalizedName)
            pod {
                $"{normalizedName}-pod"
                _labels [matchOn]
                container {
                    "nginx"
                    image "nginx"
                }
            }
        }
        service {
            $"{normalizedName}-service"
            _labels [matchOn]
            typeOf ServiceType.NodePort
            matchLabel matchOn
            servicePort {
                port portNumber
                protocol Protocol.TCP
            }
        }
    }

// Use the `publicApi` function to create 2 lists or resources and combine them into a new `K8s` instance. 
let k8sConfig = k8s {
    publicApi "Checkout" 80
    publicApi "Payments" 81
 }

// Get the output content and validation errors
let k8sOutput = KubeCtlWriter.toYaml k8sConfig

(**
### Output
*)
(*** hide ***)
makeYaml k8sOutput.content
(***include-it-raw***)