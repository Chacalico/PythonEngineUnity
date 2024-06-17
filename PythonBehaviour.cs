using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PythonEngineUnity
{
    public sealed class PythonBehaviour : MonoBehaviour
    {

        public string behaviourClass = "Behaviour";
        public PythonScript behaviourScript;

        dynamic _behaviour;

        private void OnEnable()
        {
            behaviour?.onEnable();
        }

        private void OnDisable()
        {
            behaviour?.onDisable();
        }

        private void OnDestroy()
        {
            behaviour?.onDestroy();
        }

        void Awake()
        {
            behaviour?.awake();
        }

        void Start()
        {
            behaviour?.start();
        }

        void Update()
        {
            behaviour?.update();
        }

        void LateUpdate()
        {
            behaviour?.lateUpdate();
        }

        private void OnTriggerEnter(Collider other)
        {
            behaviour?.onTriggerEnter(other);
        }

        private void OnTriggerStay(Collider other)
        {
            behaviour?.onTriggerStay(other);
        }

        private void OnTriggerExit(Collider other)
        {
            behaviour?.onTriggerExit(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            behaviour?.onCollisionEnter(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            behaviour?.onCollisionStay(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            behaviour?.onCollisionExit(collision);
        }

        public dynamic behaviour
        {
            get
            {
                if (_behaviour == null)
                {
                    _behaviour = behaviourScript.Instantiate(out ScriptScope scope, behaviourClass + "()");
                    _behaviour.gameObject = gameObject;
                    _behaviour.transform = transform;
                    _behaviour.engine = scope.Engine;
                    _behaviour.scope = scope;
                    _behaviour.behaviour = this;
                    behaviourScript.ApplyParametersToDict(_behaviour);
                    this.scope = scope;
                }
                return _behaviour;
            }
        }

        ScriptScope scope { get; set; }

    }
}
