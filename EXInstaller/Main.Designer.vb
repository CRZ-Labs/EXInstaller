<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Main
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.ProgressBarStatus = New System.Windows.Forms.ProgressBar()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.lblCurrentStatus = New System.Windows.Forms.Label()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblSubTitle = New System.Windows.Forms.Label()
        Me.lblStatusStatus = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'ProgressBarStatus
        '
        Me.ProgressBarStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ProgressBarStatus.Location = New System.Drawing.Point(12, 123)
        Me.ProgressBarStatus.Name = "ProgressBarStatus"
        Me.ProgressBarStatus.Size = New System.Drawing.Size(470, 23)
        Me.ProgressBarStatus.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        Me.ProgressBarStatus.TabIndex = 0
        '
        'lblTitle
        '
        Me.lblTitle.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblTitle.Font = New System.Drawing.Font("Arial Rounded MT Bold", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitle.Location = New System.Drawing.Point(12, 9)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(470, 26)
        Me.lblTitle.TabIndex = 1
        Me.lblTitle.Text = "Instalando..."
        '
        'lblCurrentStatus
        '
        Me.lblCurrentStatus.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblCurrentStatus.AutoSize = True
        Me.lblCurrentStatus.Location = New System.Drawing.Point(12, 107)
        Me.lblCurrentStatus.Name = "lblCurrentStatus"
        Me.lblCurrentStatus.Size = New System.Drawing.Size(46, 13)
        Me.lblCurrentStatus.TabIndex = 2
        Me.lblCurrentStatus.Text = "Estado: "
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.Location = New System.Drawing.Point(64, 107)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(418, 13)
        Me.lblStatus.TabIndex = 3
        Me.lblStatus.Text = "Esperando..."
        '
        'lblSubTitle
        '
        Me.lblSubTitle.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSubTitle.Location = New System.Drawing.Point(20, 35)
        Me.lblSubTitle.Name = "lblSubTitle"
        Me.lblSubTitle.Size = New System.Drawing.Size(454, 72)
        Me.lblSubTitle.TabIndex = 4
        Me.lblSubTitle.Text = "Espere mientras la instalación del programa se completa."
        '
        'lblStatusStatus
        '
        Me.lblStatusStatus.Location = New System.Drawing.Point(12, 149)
        Me.lblStatusStatus.Name = "lblStatusStatus"
        Me.lblStatusStatus.Size = New System.Drawing.Size(470, 13)
        Me.lblStatusStatus.TabIndex = 5
        Me.lblStatusStatus.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(494, 171)
        Me.Controls.Add(Me.lblStatusStatus)
        Me.Controls.Add(Me.lblSubTitle)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.lblCurrentStatus)
        Me.Controls.Add(Me.lblTitle)
        Me.Controls.Add(Me.ProgressBarStatus)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Main"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Instalador"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ProgressBarStatus As System.Windows.Forms.ProgressBar
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents lblCurrentStatus As System.Windows.Forms.Label
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lblSubTitle As System.Windows.Forms.Label
    Friend WithEvents lblStatusStatus As System.Windows.Forms.Label

End Class
