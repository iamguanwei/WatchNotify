namespace WatchNotifyUi
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            gb_监视 = new GroupBox();
            bt_配置_停止 = new Button();
            bt_监视_启动 = new Button();
            tb_监视 = new TextBox();
            gb_配置 = new GroupBox();
            pan_配置 = new Panel();
            bt_配置_测试发送地址 = new Button();
            bt_配置_还原 = new Button();
            bt_配置_保存 = new Button();
            tb_配置_监视清单 = new TextBox();
            lab_配置_监视清单 = new Label();
            tb_配置_消息发送地址 = new TextBox();
            lab_配置_消息发送地址 = new Label();
            ni_托盘图标 = new NotifyIcon(components);
            cms_托盘菜单 = new ContextMenuStrip(components);
            tsmi_打开 = new ToolStripMenuItem();
            tsmi_开启监控 = new ToolStripMenuItem();
            tsmi_开机启动 = new ToolStripMenuItem();
            tss_分割线 = new ToolStripSeparator();
            tsmi_退出 = new ToolStripMenuItem();
            tt_提示 = new ToolTip(components);
            toolStripMenuItem1 = new ToolStripSeparator();
            gb_监视.SuspendLayout();
            gb_配置.SuspendLayout();
            pan_配置.SuspendLayout();
            cms_托盘菜单.SuspendLayout();
            SuspendLayout();
            // 
            // gb_监视
            // 
            gb_监视.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gb_监视.Controls.Add(bt_配置_停止);
            gb_监视.Controls.Add(bt_监视_启动);
            gb_监视.Controls.Add(tb_监视);
            gb_监视.Location = new Point(12, 12);
            gb_监视.Name = "gb_监视";
            gb_监视.Size = new Size(776, 322);
            gb_监视.TabIndex = 0;
            gb_监视.TabStop = false;
            gb_监视.Text = "监视";
            // 
            // bt_配置_停止
            // 
            bt_配置_停止.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            bt_配置_停止.Location = new Point(695, 293);
            bt_配置_停止.Name = "bt_配置_停止";
            bt_配置_停止.Size = new Size(75, 23);
            bt_配置_停止.TabIndex = 1;
            bt_配置_停止.Text = "停止";
            bt_配置_停止.UseVisualStyleBackColor = true;
            // 
            // bt_监视_启动
            // 
            bt_监视_启动.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            bt_监视_启动.Location = new Point(614, 293);
            bt_监视_启动.Name = "bt_监视_启动";
            bt_监视_启动.Size = new Size(75, 23);
            bt_监视_启动.TabIndex = 1;
            bt_监视_启动.Text = "启动";
            bt_监视_启动.UseVisualStyleBackColor = true;
            // 
            // tb_监视
            // 
            tb_监视.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tb_监视.Location = new Point(6, 22);
            tb_监视.Multiline = true;
            tb_监视.Name = "tb_监视";
            tb_监视.ReadOnly = true;
            tb_监视.ScrollBars = ScrollBars.Vertical;
            tb_监视.Size = new Size(764, 265);
            tb_监视.TabIndex = 0;
            // 
            // gb_配置
            // 
            gb_配置.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gb_配置.Controls.Add(pan_配置);
            gb_配置.Location = new Point(12, 340);
            gb_配置.Name = "gb_配置";
            gb_配置.Size = new Size(776, 240);
            gb_配置.TabIndex = 1;
            gb_配置.TabStop = false;
            gb_配置.Text = "配置";
            // 
            // pan_配置
            // 
            pan_配置.Controls.Add(bt_配置_测试发送地址);
            pan_配置.Controls.Add(bt_配置_还原);
            pan_配置.Controls.Add(bt_配置_保存);
            pan_配置.Controls.Add(tb_配置_监视清单);
            pan_配置.Controls.Add(lab_配置_监视清单);
            pan_配置.Controls.Add(tb_配置_消息发送地址);
            pan_配置.Controls.Add(lab_配置_消息发送地址);
            pan_配置.Dock = DockStyle.Fill;
            pan_配置.Location = new Point(3, 19);
            pan_配置.Name = "pan_配置";
            pan_配置.Size = new Size(770, 218);
            pan_配置.TabIndex = 0;
            // 
            // bt_配置_测试发送地址
            // 
            bt_配置_测试发送地址.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            bt_配置_测试发送地址.Location = new Point(684, 32);
            bt_配置_测试发送地址.Name = "bt_配置_测试发送地址";
            bt_配置_测试发送地址.Size = new Size(75, 23);
            bt_配置_测试发送地址.TabIndex = 5;
            bt_配置_测试发送地址.Text = "测试";
            bt_配置_测试发送地址.UseVisualStyleBackColor = true;
            // 
            // bt_配置_还原
            // 
            bt_配置_还原.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            bt_配置_还原.Location = new Point(684, 188);
            bt_配置_还原.Name = "bt_配置_还原";
            bt_配置_还原.Size = new Size(75, 23);
            bt_配置_还原.TabIndex = 4;
            bt_配置_还原.Text = "还原";
            bt_配置_还原.UseVisualStyleBackColor = true;
            // 
            // bt_配置_保存
            // 
            bt_配置_保存.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            bt_配置_保存.Location = new Point(603, 188);
            bt_配置_保存.Name = "bt_配置_保存";
            bt_配置_保存.Size = new Size(75, 23);
            bt_配置_保存.TabIndex = 4;
            bt_配置_保存.Text = "保存";
            bt_配置_保存.UseVisualStyleBackColor = true;
            // 
            // tb_配置_监视清单
            // 
            tb_配置_监视清单.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tb_配置_监视清单.Location = new Point(11, 94);
            tb_配置_监视清单.Multiline = true;
            tb_配置_监视清单.Name = "tb_配置_监视清单";
            tb_配置_监视清单.ScrollBars = ScrollBars.Vertical;
            tb_配置_监视清单.Size = new Size(748, 88);
            tb_配置_监视清单.TabIndex = 3;
            // 
            // lab_配置_监视清单
            // 
            lab_配置_监视清单.AutoSize = true;
            lab_配置_监视清单.Location = new Point(11, 74);
            lab_配置_监视清单.Name = "lab_配置_监视清单";
            lab_配置_监视清单.Size = new Size(68, 17);
            lab_配置_监视清单.TabIndex = 2;
            lab_配置_监视清单.Text = "监视清单：";
            // 
            // tb_配置_消息发送地址
            // 
            tb_配置_消息发送地址.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tb_配置_消息发送地址.Location = new Point(11, 32);
            tb_配置_消息发送地址.Name = "tb_配置_消息发送地址";
            tb_配置_消息发送地址.Size = new Size(667, 23);
            tb_配置_消息发送地址.TabIndex = 1;
            // 
            // lab_配置_消息发送地址
            // 
            lab_配置_消息发送地址.AutoSize = true;
            lab_配置_消息发送地址.Location = new Point(11, 12);
            lab_配置_消息发送地址.Name = "lab_配置_消息发送地址";
            lab_配置_消息发送地址.Size = new Size(92, 17);
            lab_配置_消息发送地址.TabIndex = 0;
            lab_配置_消息发送地址.Text = "消息发送地址：";
            // 
            // ni_托盘图标
            // 
            ni_托盘图标.ContextMenuStrip = cms_托盘菜单;
            ni_托盘图标.Icon = (Icon)resources.GetObject("ni_托盘图标.Icon");
            ni_托盘图标.Visible = true;
            // 
            // cms_托盘菜单
            // 
            cms_托盘菜单.Items.AddRange(new ToolStripItem[] { tsmi_打开, toolStripMenuItem1, tsmi_开启监控, tsmi_开机启动, tss_分割线, tsmi_退出 });
            cms_托盘菜单.Name = "cms_托盘菜单";
            cms_托盘菜单.Size = new Size(181, 126);
            // 
            // tsmi_打开
            // 
            tsmi_打开.Name = "tsmi_打开";
            tsmi_打开.Size = new Size(180, 22);
            tsmi_打开.Text = "打开(&O)";
            // 
            // tsmi_开启监控
            // 
            tsmi_开启监控.Name = "tsmi_开启监控";
            tsmi_开启监控.Size = new Size(180, 22);
            tsmi_开启监控.Text = "开启监控(&M)";
            // 
            // tsmi_开机启动
            // 
            tsmi_开机启动.Name = "tsmi_开机启动";
            tsmi_开机启动.Size = new Size(180, 22);
            tsmi_开机启动.Text = "开机启动(&S)";
            // 
            // tss_分割线
            // 
            tss_分割线.Name = "tss_分割线";
            tss_分割线.Size = new Size(177, 6);
            // 
            // tsmi_退出
            // 
            tsmi_退出.Name = "tsmi_退出";
            tsmi_退出.Size = new Size(180, 22);
            tsmi_退出.Text = "退出(&E)";
            // 
            // tt_提示
            // 
            tt_提示.ToolTipIcon = ToolTipIcon.Info;
            tt_提示.ToolTipTitle = "格式说明";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(177, 6);
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 592);
            Controls.Add(gb_配置);
            Controls.Add(gb_监视);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            gb_监视.ResumeLayout(false);
            gb_监视.PerformLayout();
            gb_配置.ResumeLayout(false);
            pan_配置.ResumeLayout(false);
            pan_配置.PerformLayout();
            cms_托盘菜单.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox gb_监视;
        private TextBox tb_监视;
        private GroupBox gb_配置;
        private Panel pan_配置;
        private TextBox tb_配置_消息发送地址;
        private Label lab_配置_消息发送地址;
        private Label lab_配置_监视清单;
        private Button bt_配置_停止;
        private Button bt_监视_启动;
        private TextBox tb_配置_监视清单;
        private NotifyIcon ni_托盘图标;
        private ContextMenuStrip cms_托盘菜单;
        private ToolStripMenuItem tsmi_打开;
        private ToolStripMenuItem tsmi_开启监控;
        private ToolStripMenuItem tsmi_开机启动;
        private ToolStripSeparator tss_分割线;
        private ToolStripMenuItem tsmi_退出;
        private Button bt_配置_还原;
        private Button bt_配置_保存;
        private ToolTip tt_提示;
        private Button bt_配置_测试发送地址;
        private ToolStripSeparator toolStripMenuItem1;
    }
}
