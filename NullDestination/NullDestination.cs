using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

using IDTSOutput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutput100;
using IDTSCustomProperty = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSCustomProperty100;
using IDTSOutputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSOutputColumn100;
using IDTSInput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInput100;
using IDTSInputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumn100;
using IDTSVirtualInputColumn = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInputColumn100;
using IDTSVirtualInput = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSVirtualInput100;
using IDTSInputColumnCollection = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSInputColumnCollection100;
using IDTSComponentMetaData = Microsoft.SqlServer.Dts.Pipeline.Wrapper.IDTSComponentMetaData100;

namespace Martin.SQLServer.Dts
{
    [DtsPipelineComponent(
           DisplayName = "Null Destination",
           Description = "Destination that just dumps all the records into a null output.",
           IconResource = "Martin.SQLServer.Dts.Null.ico", // Icons sourced from Iconsmind.com
           ComponentType = ComponentType.DestinationAdapter,
           CurrentVersion = 0)]
    public class NullDestination : PipelineComponent
    {
        /// <summary>
        /// stores whether there are issues with input columns as a result from Validate.
        /// </summary>
        private bool areInputColumnsValid = true;

        /// <summary>
        /// Sets the basic infirmation about the component.
        /// </summary>
        public override void ProvideComponentProperties()
        {
            // Remove anything that shouldn't be here.
            this.RemoveAllInputsOutputsAndCustomProperties();
            // Remove any connection collections.
            ComponentMetaData.RuntimeConnectionCollection.RemoveAll();
            // Create an input
            ComponentMetaData.InputCollection.New();
            // Assign a name.
            ComponentMetaData.InputCollection[0].Name = "TrashInput";
            // Set the contact information.
            ComponentMetaData.ContactInfo = "https://github.com/keif888/NullDestination/";
        }

        /// <summary>
        /// Upgrade the metadata if it needs it.
        /// Right now all this does is update the version number in the XML.
        /// </summary>
        /// <param name="pipelineVersion">The curreht version of the pipeline.</param>
        public override void PerformUpgrade(int pipelineVersion)
        {
            // Get the attributes for the executable
            DtsPipelineComponentAttribute componentAttribute = (DtsPipelineComponentAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(DtsPipelineComponentAttribute), false);
            int binaryVersion = componentAttribute.CurrentVersion;

            // Set the SSIS Package's version ID for this component to the binary version...
            ComponentMetaData.Version = binaryVersion;
        }

        /// <summary>
        /// Called repeatedly when the component is edited in the designer, and once at the beginning of execution.
        /// Verifies the following:
        /// 1. Check that there are no outputs
        /// 2. Check that there is only one input
        /// 3. Check that all upstream columns are present.
        /// </summary>
        /// <returns>The status of the validation</returns>
        public override DTSValidationStatus Validate()
        {
            if (ComponentMetaData.InputCollection.Count != 1)
            {
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            if (ComponentMetaData.OutputCollection.Count != 0)
            {
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            IDTSInput input = ComponentMetaData.InputCollection[0];
            IDTSVirtualInput vInput = input.GetVirtualInput();
            bool cancel = false;
            foreach(IDTSInputColumn inputColumn in input.InputColumnCollection)
            {
                try
                {
                    IDTSVirtualInputColumn vColumn = vInput.VirtualInputColumnCollection.GetVirtualInputColumnByLineageID(inputColumn.LineageID);
                }
                catch
                {
                    ComponentMetaData.FireError(0, ComponentMetaData.Name, "The input column " + inputColumn.IdentificationString + " does not match a column in the upstream output.", String.Empty, 0, out cancel);
                    areInputColumnsValid = false;
                    return DTSValidationStatus.VS_NEEDSNEWMETADATA;
                }
            }
            return base.Validate();
        }

        /// <summary>
        /// If there are validation issues, then repair them!
        /// The only issue that can be repaired is when there are missing upstream columns.
        /// </summary>
        public override void ReinitializeMetaData()
        {
            if (!areInputColumnsValid)
            {
                int inputID = ComponentMetaData.InputCollection[0].ID;
                // Remove all the current columns.
                ComponentMetaData.InputCollection[0].InputColumnCollection.RemoveAll();
                // Loop though all the columns in the input path, and connect them.
                IDTSVirtualInput virtualInput = ComponentMetaData.InputCollection[0].GetVirtualInput();
                if (virtualInput == null)
                {
                    throw new ArgumentNullException("virtualInput");
                }

                foreach (IDTSVirtualInputColumn viColumn in virtualInput.VirtualInputColumnCollection)
                {
                    this.SetUsageType(inputID, virtualInput, viColumn.LineageID, DTSUsageType.UT_READONLY);
                }
                areInputColumnsValid = true;
            }
            base.ReinitializeMetaData();
        }

        /// <summary>
        /// If a path is connected, automatically select all the columns.
        /// </summary>
        /// <param name="inputID">The internal id of the input.  Should always translate to index 0, but...</param>
        public override void OnInputPathAttached(int inputID)
        {
            // Get the index of the input in the collection
            int inputIndex = ComponentMetaData.InputCollection.FindObjectIndexByID(inputID);
            // Remove all the current columns.
            ComponentMetaData.InputCollection[inputIndex].InputColumnCollection.RemoveAll();
            // Loop though all the columns in the input path, and connect them.
            IDTSVirtualInput virtualInput = ComponentMetaData.InputCollection[inputIndex].GetVirtualInput();
            if (virtualInput == null)
            {
                throw new ArgumentNullException("virtualInput");
            }

            foreach(IDTSVirtualInputColumn viColumn in virtualInput.VirtualInputColumnCollection)
            {
                this.SetUsageType(inputID, virtualInput, viColumn.LineageID, DTSUsageType.UT_READONLY);
            }
        }

        /// <summary>
        /// When an input is detached, remove all the columns in the input's collection.
        /// </summary>
        /// <param name="inputID">The internal id of the input.  Should always translate to index 0, but...</param>
        public override void OnInputPathDetached(int inputID)
        {
            int inputIndex = ComponentMetaData.InputCollection.FindObjectIndexByID(inputID);
            ComponentMetaData.InputCollection[inputIndex].InputColumnCollection.RemoveAll();
        }

        /// <summary>
        /// For every record received, do nothing.
        /// </summary>
        /// <param name="inputID">The internal id of the input.  Should always translate to index 0, but...</param>
        /// <param name="buffer">The SSIS buffer that contains the data that is supposed to be processed.</param>
        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            while(buffer.NextRow())
            {
                // Do Nothing
            }
        }

    }
}
