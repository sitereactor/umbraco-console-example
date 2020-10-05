﻿using System;
using System.Globalization;
using System.Management.Automation.Host;

namespace UmbConsole
{
    /// <summary>
    /// This is a sample implementation of the PSHost abstract class for
    /// console applications. Not all members are implemented. Those that
    /// are not implemented throw a NotImplementedException exception or
    /// return nothing.
    /// </summary>
    internal class MyHost : PSHost
    {
        /// <summary>
        /// A reference to the PSHost implementation.
        /// </summary>
        private PSListenerConsoleSample program;

        /// <summary>
        /// The culture information of the thread that created
        /// this object.
        /// </summary>
        private CultureInfo originalCultureInfo =
            System.Threading.Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// The UI culture information of the thread that created
        /// this object.
        /// </summary>
        private CultureInfo originalUICultureInfo =
            System.Threading.Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// The identifier of this PSHost implementation.
        /// </summary>
        private static Guid instanceId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the MyHost class. Keep
        /// a reference to the host application object so that it
        /// can be informed of when to exit.
        /// </summary>
        /// <param name="program">
        /// A reference to the host application object.
        /// </param>
        public MyHost(PSListenerConsoleSample program)
        {
            this.program = program;
        }

        /// <summary>
        /// A reference to the implementation of the PSHostUserInterface
        /// class for this application.
        /// </summary>
        private MyHostUserInterface myHostUserInterface = new MyHostUserInterface();

        /// <summary>
        /// Gets the culture information to use. This implementation
        /// returns a snapshot of the culture information of the thread
        /// that created this object.
        /// </summary>
        public override CultureInfo CurrentCulture
        {
            get { return this.originalCultureInfo; }
        }

        /// <summary>
        /// Gets the UI culture information to use. This implementation
        /// returns a snapshot of the UI culture information of the thread
        /// that created this object.
        /// </summary>
        public override CultureInfo CurrentUICulture
        {
            get { return this.originalUICultureInfo; }
        }

        /// <summary>
        /// Gets an identifier for this host. This implementation always
        /// returns the GUID allocated at instantiation time.
        /// </summary>
        public override Guid InstanceId
        {
            get { return instanceId; }
        }

        /// <summary>
        /// Gets a string that contains the name of this host implementation.
        /// Keep in mind that this string may be used by script writers to
        /// identify when your host is being used.
        /// </summary>
        public override string Name
        {
            get { return "UmbracoPowerShell"; }
        }

        /// <summary>
        /// Gets an instance of the implementation of the PSHostUserInterface
        /// class for this application. This instance is allocated once at startup time
        /// and returned every time thereafter.
        /// </summary>
        public override PSHostUserInterface UI
        {
            get { return this.myHostUserInterface; }
        }

        /// <summary>
        /// Gets the version object for this application. Typically this
        /// should match the version resource in the application.
        /// </summary>
        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        /// <summary>
        /// This API Instructs the host to interrupt the currently running
        /// pipeline and start a new nested input loop. In this example this
        /// functionality is not needed so the method throws a
        /// NotImplementedException exception.
        /// </summary>
        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException(
                  "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API instructs the host to exit the currently running input loop.
        /// In this example this functionality is not needed so the method
        /// throws a NotImplementedException exception.
        /// </summary>
        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException(
                  "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API is called before an external application process is
        /// started. Typically it is used to save state so that the parent
        /// can restore state that has been modified by a child process (after
        /// the child exits). In this example this functionality is not
        /// needed so the method returns nothing.
        /// </summary>
        public override void NotifyBeginApplication()
        {
            return;
        }

        /// <summary>
        /// This API is called after an external application process finishes.
        /// Typically it is used to restore state that a child process has
        /// altered. In this example, this functionality is not needed so
        /// the method returns nothing.
        /// </summary>
        public override void NotifyEndApplication()
        {
            return;
        }

        /// <summary>
        /// Indicate to the host application that exit has
        /// been requested. Pass the exit code that the host
        /// application should use when exiting the process.
        /// </summary>
        /// <param name="exitCode">The exit code that the
        /// host application should use.</param>
        public override void SetShouldExit(int exitCode)
        {
            this.program.ShouldExit = true;
            this.program.ExitCode = exitCode;
        }
    }
}