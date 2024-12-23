namespace GUI.Controls;

partial class SoundEventsList
{
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        soundEntriesPanel = new System.Windows.Forms.FlowLayoutPanel();
        searchTextBox = new System.Windows.Forms.TextBox();
        SuspendLayout();
        // 
        // soundEntriesPanel
        // 
        soundEntriesPanel.AutoScroll = true;
        soundEntriesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        soundEntriesPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        soundEntriesPanel.Location = new System.Drawing.Point(0, 23);
        soundEntriesPanel.Name = "soundEntriesPanel";
        soundEntriesPanel.Size = new System.Drawing.Size(701, 430);
        soundEntriesPanel.TabIndex = 1;
        soundEntriesPanel.WrapContents = false;
        // 
        // searchTextBox
        // 
        searchTextBox.Dock = System.Windows.Forms.DockStyle.Top;
        searchTextBox.Location = new System.Drawing.Point(0, 0);
        searchTextBox.Name = "searchTextBox";
        searchTextBox.PlaceholderText = "Filter soundevents...";
        searchTextBox.Size = new System.Drawing.Size(701, 23);
        searchTextBox.TabIndex = 0;
        searchTextBox.TextChanged += searchTextBox_TextChanged;
        // 
        // SoundEventsList
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        Controls.Add(soundEntriesPanel);
        Controls.Add(searchTextBox);
        Name = "SoundEventsList";
        Size = new System.Drawing.Size(701, 453);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.FlowLayoutPanel soundEntriesPanel;
    private System.Windows.Forms.TextBox searchTextBox;
}
