namespace Kurogane.SimpleEditor {
	partial class StartForm {
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent() {
			this.txtProgram = new System.Windows.Forms.TextBox();
			this.txtOut = new System.Windows.Forms.TextBox();
			this.lblProgram = new System.Windows.Forms.Label();
			this.lblOut = new System.Windows.Forms.Label();
			this.btnExecute = new System.Windows.Forms.Button();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.btnOpen = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtProgram
			// 
			this.txtProgram.AcceptsReturn = true;
			this.txtProgram.AcceptsTab = true;
			this.txtProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtProgram.Font = new System.Drawing.Font("ＭＳ ゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.txtProgram.ImeMode = System.Windows.Forms.ImeMode.On;
			this.txtProgram.Location = new System.Drawing.Point(5, 32);
			this.txtProgram.Multiline = true;
			this.txtProgram.Name = "txtProgram";
			this.txtProgram.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtProgram.Size = new System.Drawing.Size(377, 533);
			this.txtProgram.TabIndex = 0;
			this.txtProgram.Text = "「こんにちは」を出力する。";
			this.txtProgram.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtProgram_KeyDown);
			// 
			// txtOut
			// 
			this.txtOut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtOut.BackColor = System.Drawing.Color.White;
			this.txtOut.Font = new System.Drawing.Font("ＭＳ ゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.txtOut.Location = new System.Drawing.Point(5, 32);
			this.txtOut.Multiline = true;
			this.txtOut.Name = "txtOut";
			this.txtOut.ReadOnly = true;
			this.txtOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtOut.Size = new System.Drawing.Size(388, 533);
			this.txtOut.TabIndex = 1;
			this.txtOut.TabStop = false;
			// 
			// lblProgram
			// 
			this.lblProgram.AutoSize = true;
			this.lblProgram.Location = new System.Drawing.Point(3, 8);
			this.lblProgram.Name = "lblProgram";
			this.lblProgram.Size = new System.Drawing.Size(50, 12);
			this.lblProgram.TabIndex = 2;
			this.lblProgram.Text = "プログラム";
			// 
			// lblOut
			// 
			this.lblOut.AutoSize = true;
			this.lblOut.Location = new System.Drawing.Point(3, 8);
			this.lblOut.Name = "lblOut";
			this.lblOut.Size = new System.Drawing.Size(53, 12);
			this.lblOut.TabIndex = 3;
			this.lblOut.Text = "実行結果";
			// 
			// btnExecute
			// 
			this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExecute.Location = new System.Drawing.Point(307, 3);
			this.btnExecute.Name = "btnExecute";
			this.btnExecute.Size = new System.Drawing.Size(75, 23);
			this.btnExecute.TabIndex = 3;
			this.btnExecute.Text = "実行";
			this.btnExecute.UseVisualStyleBackColor = true;
			this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
			// 
			// splitContainer
			// 
			this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer.Location = new System.Drawing.Point(12, 12);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.btnOpen);
			this.splitContainer.Panel1.Controls.Add(this.btnSave);
			this.splitContainer.Panel1.Controls.Add(this.lblProgram);
			this.splitContainer.Panel1.Controls.Add(this.btnExecute);
			this.splitContainer.Panel1.Controls.Add(this.txtProgram);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.lblOut);
			this.splitContainer.Panel2.Controls.Add(this.txtOut);
			this.splitContainer.Size = new System.Drawing.Size(785, 568);
			this.splitContainer.SplitterDistance = 385;
			this.splitContainer.TabIndex = 5;
			this.splitContainer.TabStop = false;
			// 
			// btnOpen
			// 
			this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOpen.Location = new System.Drawing.Point(145, 3);
			this.btnOpen.Name = "btnOpen";
			this.btnOpen.Size = new System.Drawing.Size(75, 23);
			this.btnOpen.TabIndex = 1;
			this.btnOpen.Text = "開く";
			this.btnOpen.UseVisualStyleBackColor = true;
			this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
			// 
			// btnSave
			// 
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSave.Location = new System.Drawing.Point(226, 3);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(75, 23);
			this.btnSave.TabIndex = 2;
			this.btnSave.Text = "保存";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// StartForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(809, 592);
			this.Controls.Add(this.splitContainer);
			this.Name = "StartForm";
			this.Text = "クロガネエディタ";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtProgram_KeyDown);
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel1.PerformLayout();
			this.splitContainer.Panel2.ResumeLayout(false);
			this.splitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox txtProgram;
		private System.Windows.Forms.TextBox txtOut;
		private System.Windows.Forms.Label lblProgram;
		private System.Windows.Forms.Label lblOut;
		private System.Windows.Forms.Button btnExecute;
		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnOpen;
	}
}

