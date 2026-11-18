using System.IO;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using TechGuildEventManager.Interfaces;
using System.Windows.Forms;
using TechGuildEventManager.Models;

namespace TechGuildEventManager.Forms;

public class MainForm : Form
{
    private readonly Color darkBrown = Color.FromArgb(78, 52, 36);
    private readonly Color brown = Color.FromArgb(130, 91, 63);
    private readonly Color beige = Color.FromArgb(220, 201, 178);
    private readonly Color cream = Color.FromArgb(247, 242, 235);
    private readonly Color white = Color.White;

    private readonly IGuildDataService _dataService;

    private List<GuildMember> members => _dataService.Members;

    private List<GuildEvent> eventsList => _dataService.Events;

    private List<TechCompetition> competitions => _dataService.Competitions;
    private Panel content = new();
    private Label pageTitle = new();
    private readonly List<Button> navButtons = new();
    private int nextMemberId = 1;
    private int nextEventId = 1;
    private int nextCompetitionId = 1;

    public MainForm(IGuildDataService dataService)
    {
        _dataService = dataService;

        Text = "Tech Guild & Event Manager";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 650);
        Size = new Size(1250, 780);
        BackColor = cream;
        Font = new Font("Segoe UI", 10F);

        BuildLayout();

        LoadAllData();

        if (members.Count == 0 &&
            eventsList.Count == 0 &&
            competitions.Count == 0)
        {
            SeedSampleData();
            SaveAllData();
        }

        ShowDashboard();
    }

    private void BuildLayout()
    {
        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 250,
            BackColor = darkBrown
        };

        Controls.Add(sidebar);

        var brand = new Label
        {
            Text = "TECH GUILD\nEVENT\nMANAGER",
            ForeColor = white,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(18, 20),
            Size = new Size(215, 105),
            TextAlign = ContentAlignment.MiddleLeft
        };

        sidebar.Controls.Add(brand);

        var nav = new TableLayoutPanel
        {
            Location = new Point(12, 140),
            Size = new Size(236, 330),
            ColumnCount = 1,
            RowCount = 5,
            BackColor = darkBrown,
            Anchor = AnchorStyles.Top |
                     AnchorStyles.Left |
                     AnchorStyles.Right
        };

        nav.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < 5; i++)
        {
            nav.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        }

        sidebar.Controls.Add(nav);

        AddNavButton(nav, "Dashboard", ShowDashboard);
        AddNavButton(nav, "Member Management", ShowMembers);
        AddNavButton(nav, "Event Management", ShowEvents);
        AddNavButton(nav, "Competition Management", ShowCompetitions);
        AddNavButton(nav, "Points && Reports", ShowReports);

        var right = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = cream,
            Padding = new Padding(250, 0, 0, 0)
        };
        Controls.Add(right);

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 76,
            BackColor = white,
            Padding = new Padding(28, 12, 25, 10)
        };

        pageTitle = new Label
        {
            Text = "Dashboard",
            Dock = DockStyle.Left,
            Width = 500,
            Font = new Font("Segoe UI", 19, FontStyle.Bold),
            ForeColor = darkBrown,
            TextAlign = ContentAlignment.MiddleLeft
        };
        header.Controls.Add(pageTitle);

        var subtitle = new Label
        {
            Text = "GUILD ADMINISTRATION",
            Dock = DockStyle.Right,
            Width = 220,
            ForeColor = brown,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight
        };
        header.Controls.Add(subtitle);

        content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = cream,
            AutoScroll = true,
            Padding = new Padding(20)
        };

        right.Controls.Add(content);
        right.Controls.Add(header);
    }

    private void AddNavButton(TableLayoutPanel nav, string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Tag = text.Replace("&&", "&"),
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = darkBrown,
            ForeColor = white,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 3, 0, 3),
            Padding = new Padding(10, 0, 0, 0)
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = brown;
        button.Click += (sender, e) => action();

        navButtons.Add(button);
        nav.Controls.Add(button);
    }

    private void SetPage(string title)
    {
        string cleanTitle = title.Replace("&&", "&");

        if (cleanTitle == "Dashboard")
        {
            pageTitle.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            pageTitle.ForeColor = brown;
            pageTitle.Text = $"Overview  |  {DateTime.Now:dddd, MMMM dd, yyyy}";
        }
        else
        {
            pageTitle.Font = new Font("Segoe UI", 19, FontStyle.Bold);
            pageTitle.ForeColor = darkBrown;
            pageTitle.Text = title;
        }

        foreach (var btn in navButtons)
        {
            btn.BackColor = (btn.Tag?.ToString() == cleanTitle) ? brown : darkBrown;
        }

        content.Controls.Clear();
    }

    private Label MakeLabel(string text, int size = 10, bool bold = false)
        => new()
        {
            Text = text,
            ForeColor = darkBrown,
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            AutoSize = true
        };

    private Button MakeButton(string text, Action action, bool primary = true)
    {
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(110, 38),
            Padding = new Padding(10, 5, 10, 5),
            BackColor = primary ? brown : beige,
            ForeColor = primary ? white : darkBrown,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(5)
        };
        b.FlatAppearance.BorderSize = 0;
        b.Click += (_, _) => action();
        return b;
    }

    private DataGridView MakeGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = white,
            BorderStyle = BorderStyle.None,
            GridColor = Color.FromArgb(190, 180, 170),
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = 32,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = darkBrown,
                ForeColor = white,
                SelectionBackColor = brown,
                SelectionForeColor = white,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(7, 0, 5, 0),
                WrapMode = DataGridViewTriState.False
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = white,
                ForeColor = darkBrown,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                SelectionBackColor = beige,
                SelectionForeColor = darkBrown,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(7, 0, 5, 0),
                WrapMode = DataGridViewTriState.False
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = cream,
                ForeColor = darkBrown,
                SelectionBackColor = beige,
                SelectionForeColor = darkBrown
            },
            RowTemplate = { Height = 34 }
        };

        grid.ColumnHeaderMouseClick += (_, e) =>
        {
            if (grid.Rows.Count > 0)
            {
                int rowIndex = grid.CurrentCell?.RowIndex ?? 0;
                grid.CurrentCell = grid.Rows[rowIndex].Cells[e.ColumnIndex];
            }
        };

        return grid;
    }

    private void ShowDashboard()
    {
        SetPage("Dashboard");

        content.SuspendLayout();

        int dashboardWidth = content.ClientSize.Width - 50;

        var statsPanel = new FlowLayoutPanel
        {
            Location = new Point(25, 35),
            Size = new Size(dashboardWidth, 125),
            BackColor = cream,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0),
            Margin = new Padding(0),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        int cardWidth = (dashboardWidth - 40) / 4;

        AddStatCard(statsPanel, "TOTAL MEMBERS", members.Count.ToString(), cardWidth);
        AddStatCard(statsPanel, "OFFICERS", members.OfType<Officer>().Count().ToString(), cardWidth);
        AddStatCard(statsPanel, "EVENTS", eventsList.Count.ToString(), cardWidth);
        AddStatCard(statsPanel, "COMPETITIONS", competitions.Count.ToString(), cardWidth);

        content.Controls.Add(statsPanel);

        var membershipPanel = new FlowLayoutPanel
        {
            Location = new Point(25, 180),
            Size = new Size(dashboardWidth, 125),
            BackColor = cream,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0),
            Margin = new Padding(0),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        int memberCardWidth = (dashboardWidth - 20) / 3;

        AddStatCard(membershipPanel, "OFFICERS", members.OfType<Officer>().Count().ToString(), memberCardWidth);
        AddStatCard(membershipPanel, "REGULAR MEMBERS", members.OfType<RegularMember>().Count().ToString(), memberCardWidth);
        AddStatCard(membershipPanel, "ALUMNI", members.OfType<Alumni>().Count().ToString(), memberCardWidth);

        content.Controls.Add(membershipPanel);

        int halfWidth = (dashboardWidth - 20) / 2;

        var eventsCard = new Panel
        {
            Location = new Point(25, 325),
            Size = new Size(halfWidth, 230),
            BackColor = white,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(18),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        eventsCard.Controls.Add(new Label
        {
            Text = "Upcoming Events",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = darkBrown,
            Location = new Point(18, 16),
            AutoSize = true
        });

        var upcomingEvents = eventsList.OrderBy(e => e.EventDate).Take(4).ToList();
        if (upcomingEvents.Count == 0)
        {
            eventsCard.Controls.Add(new Label
            {
                Text = "No scheduled events yet.",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = brown,
                Location = new Point(18, 55),
                AutoSize = true
            });
        }
        else
        {
            int yPos = 55;
            foreach (var ev in upcomingEvents)
            {
                eventsCard.Controls.Add(new Label
                {
                    Text = $"•  {ev.EventName} — {ev.EventDate:MMM dd, yyyy} ({ev.Venue})",
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = darkBrown,
                    AutoEllipsis = true,
                    Location = new Point(18, yPos),
                    Size = new Size(halfWidth - 36, 28)
                });
                yPos += 36;
            }
        }
        content.Controls.Add(eventsCard);

        var topMembersCard = new Panel
        {
            Location = new Point(25 + halfWidth + 20, 325),
            Size = new Size(halfWidth, 230),
            BackColor = white,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(18),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        topMembersCard.Controls.Add(new Label
        {
            Text = "Top Guild Members (Base Points)",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = darkBrown,
            Location = new Point(18, 16),
            AutoSize = true
        });

        var topMembers = members
            .Select(m => new { m.FullName, m.MemberType, Points = m.CalculateEventPoints(1) })
            .OrderByDescending(x => x.Points)
            .Take(4)
            .ToList();

        if (topMembers.Count == 0)
        {
            topMembersCard.Controls.Add(new Label
            {
                Text = "No members added yet.",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = brown,
                Location = new Point(18, 55),
                AutoSize = true
            });
        }
        else
        {
            int yPos = 55;
            int rank = 1;
            foreach (var tm in topMembers)
            {
                topMembersCard.Controls.Add(new Label
                {
                    Text = $"{rank}.  {tm.FullName} ({tm.MemberType})  —  {tm.Points} pts",
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = darkBrown,
                    AutoEllipsis = true,
                    Location = new Point(18, yPos),
                    Size = new Size(halfWidth - 36, 28)
                });
                yPos += 36;
                rank++;
            }
        }
        content.Controls.Add(topMembersCard);

        content.ResumeLayout();
    }

    private void AddStatCard(FlowLayoutPanel parent, string heading, string value, int cardWidth)
    {
        var card = new Panel
        {
            Width = cardWidth,
            Height = 112,
            BackColor = white,
            Margin = new Padding(0, 0, 10, 10),
            Padding = new Padding(15),
            BorderStyle = BorderStyle.FixedSingle
        };

        var h = new Label
        {
            Text = heading,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = brown,
            Location = new Point(15, 20),
            AutoSize = true
        };

        var v = new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 27, FontStyle.Bold),
            ForeColor = darkBrown,
            Location = new Point(15, 48),
            AutoSize = true
        };

        card.Controls.Add(h);
        card.Controls.Add(v);
        parent.Controls.Add(card);
    }

    private bool IsIdAlreadyInUse(int id, int? ignoreMemberId = null, int? ignoreEventId = null, int? ignoreCompetitionId = null) =>
        members.Any(m => m.MemberId != ignoreMemberId && m.MemberId == id) ||
        eventsList.Any(e => e.EventId != ignoreEventId && e.EventId == id) ||
        competitions.Any(c => c.CompetitionId != ignoreCompetitionId && c.CompetitionId == id);

    private void ShowMembers()
    {
        SetPage("Member Management");

        var tools = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = cream,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 0)
        };

        var searchBox = new TextBox
        {
            Width = 180,
            Margin = new Padding(5, 11, 5, 5)
        };

        var filterBox = new ComboBox
        {
            Width = 140,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(5, 11, 5, 5)
        };

        filterBox.Items.AddRange(new object[]
        {
            "All Roles",
            "Officer",
            "Regular Member",
            "Alumni"
        });

        filterBox.SelectedIndex = 0;

        var grid = MakeGrid();

        content.Controls.Add(grid);
        content.Controls.Add(tools);

        void Refresh()
        {
            string keyword = searchBox.Text.Trim();
            string selectedType = filterBox.Text;

            var result = members.Where(m =>
                (m.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 m.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 m.Course.Contains(keyword, StringComparison.OrdinalIgnoreCase)) &&
                (selectedType == "All Roles" || m.MemberType == selectedType)
            ).Select(m => new
            {
                m.MemberId,
                m.FullName,
                m.Email,
                m.Course,
                Type = m.MemberType
            }).ToList();

            grid.DataSource = null;
            grid.DataSource = result;
        }

        tools.Controls.Add(MakeButton("+ Add Member", () =>
        {
            using var dialog = new MemberDialog(members, eventsList, competitions);

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var created = dialog.CreateMember();
                if (IsIdAlreadyInUse(created.MemberId))
                {
                    MessageBox.Show($"ID {created.MemberId} already exists. Cannot save.",
                        "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                members.Add(created);
                SaveAllData();
                Refresh();
            }
        }));

        tools.Controls.Add(MakeButton("Edit Selected", () =>
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Select a member first.");
                return;
            }

            int id = Convert.ToInt32(grid.CurrentRow.Cells["MemberId"].Value);
            var member = members.FirstOrDefault(m => m.MemberId == id);
            if (member == null) return;

            using var dialog = new MemberDialog(members, eventsList, competitions, member);

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var updated = dialog.CreateMember();
                if (IsIdAlreadyInUse(updated.MemberId, ignoreMemberId: id))
                {
                    MessageBox.Show($"ID {updated.MemberId} already exists. Cannot save.",
                        "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int index = members.IndexOf(member);
                members[index] = updated;

                SaveAllData();
                Refresh();
            }
        }, false));

        tools.Controls.Add(MakeButton("Delete Selected", () =>
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Select a member first.");
                return;
            }

            int id = Convert.ToInt32(grid.CurrentRow.Cells["MemberId"].Value);
            var member = members.FirstOrDefault(m => m.MemberId == id);

            if (member != null &&
                MessageBox.Show("Delete this member?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                members.Remove(member);
                SaveAllData();
                Refresh();
            }
        }, false));

        tools.Controls.Add(new Label
        {
            Text = "Search:",
            AutoSize = true,
            Margin = new Padding(8, 14, 0, 0)
        });

        tools.Controls.Add(searchBox);

        tools.Controls.Add(new Label
        {
            Text = "Filter:",
            AutoSize = true,
            Margin = new Padding(8, 14, 0, 0)
        });

        tools.Controls.Add(filterBox);

        searchBox.TextChanged += (_, _) => Refresh();
        filterBox.SelectedIndexChanged += (_, _) => Refresh();

        Refresh();
    }

    private void ShowEvents()
    {
        SetPage("Event Management");

        var tools = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = cream,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 0)
        };

        var searchBox = new TextBox
        {
            Width = 180,
            Margin = new Padding(5, 11, 5, 5)
        };

        var grid = MakeGrid();

        content.Controls.Add(grid);
        content.Controls.Add(tools);

        void Refresh()
        {
            string keyword = searchBox.Text.Trim();

            grid.DataSource = null;

            grid.DataSource = eventsList
                .Where(e =>
                    e.EventName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    e.Venue.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    e.Organizer.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .Select(e => new
                {
                    e.EventId,
                    e.EventName,
                    Date = e.EventDate.ToString("dd MMM yyyy"),
                    e.Venue,
                    e.Organizer
                })
                .ToList();
        }

        tools.Controls.Add(MakeButton("+ Add Event", () =>
        {
            using var dialog = new EventDialog(members, eventsList, competitions);

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var created = dialog.CreateEvent();
                if (IsIdAlreadyInUse(created.EventId))
                {
                    MessageBox.Show($"ID {created.EventId} already exists. Cannot save.",
                        "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                eventsList.Add(created);
                SaveAllData();
                Refresh();
            }
        }));

        tools.Controls.Add(MakeButton("Edit Selected", () =>
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Select an event first.");
                return;
            }

            int id = Convert.ToInt32(grid.CurrentRow.Cells["EventId"].Value);
            var item = eventsList.FirstOrDefault(e => e.EventId == id);
            if (item == null) return;

            using var dialog = new EventDialog(members, eventsList, competitions, item);

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var updated = dialog.CreateEvent();
                if (IsIdAlreadyInUse(updated.EventId, ignoreEventId: id))
                {
                    MessageBox.Show($"ID {updated.EventId} already exists. Cannot save.",
                        "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int index = eventsList.IndexOf(item);
                eventsList[index] = updated;
                SaveAllData();
                Refresh();
            }
        }, false));

        tools.Controls.Add(MakeButton("Delete Selected", () =>
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Select an event first.");
                return;
            }

            int id = Convert.ToInt32(grid.CurrentRow.Cells["EventId"].Value);
            var item = eventsList.FirstOrDefault(e => e.EventId == id);

            if (item != null &&
                MessageBox.Show("Delete this event?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                eventsList.Remove(item);
                SaveAllData();
                Refresh();
            }
        }, false));

        tools.Controls.Add(new Label
        {
            Text = "Search:",
            AutoSize = true,
            Margin = new Padding(8, 14, 0, 0)
        });

        tools.Controls.Add(searchBox);

        searchBox.TextChanged += (_, _) => Refresh();

        Refresh();
    }

    private void ShowCompetitions()
    {
        SetPage("Competition Management");

        var tools = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = cream,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 0)
        };

        var searchBox = new TextBox
        {
            Width = 180,
            Margin = new Padding(5, 11, 5, 5)
        };

        var grid = MakeGrid();

        content.Controls.Add(grid);
        content.Controls.Add(tools);

        void Refresh()
        {
            string keyword = searchBox.Text.Trim();

            grid.DataSource = null;

            grid.DataSource = competitions
                .Where(c =>
                    c.CompetitionName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    c.ParticipantName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    c.TechStack.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .Select(c => new
                {
                    c.CompetitionId,
                    c.CompetitionName,
                    c.ParticipantName,
                    c.TechStack,
                    c.Rank
                })
                .ToList();
        }

        tools.Controls.Add(MakeButton("+ Add Competition", () =>
        {
            using var dialog = new CompetitionDialog(members, eventsList, competitions);

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var created = dialog.CreateCompetition();
                if (IsIdAlreadyInUse(created.CompetitionId))
                {
                    MessageBox.Show($"ID {created.CompetitionId} already exists. Cannot save.",
                        "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                competitions.Add(created);
                SaveAllData();
                Refresh();
            }
        }));

        tools.Controls.Add(MakeButton("Edit Selected", () =>
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Select a competition first.");
                return;
            }

            int id = Convert.ToInt32(grid.CurrentRow.Cells["CompetitionId"].Value);
            var item = competitions.FirstOrDefault(c => c.CompetitionId == id);
            if (item == null) return;

            using var dialog = new CompetitionDialog(members, eventsList, competitions, item);

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var updated = dialog.CreateCompetition();
                if (IsIdAlreadyInUse(updated.CompetitionId, ignoreCompetitionId: id))
                {
                    MessageBox.Show($"ID {updated.CompetitionId} already exists. Cannot save.",
                        "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int index = competitions.IndexOf(item);
                competitions[index] = updated;
                SaveAllData();
                Refresh();
            }
        }, false));

        tools.Controls.Add(MakeButton("Delete Selected", () =>
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Select a competition first.");
                return;
            }

            int id = Convert.ToInt32(grid.CurrentRow.Cells["CompetitionId"].Value);
            var item = competitions.FirstOrDefault(c => c.CompetitionId == id);

            if (item != null &&
                MessageBox.Show("Delete this competition?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                competitions.Remove(item);
                SaveAllData();
                Refresh();
            }
        }, false));

        tools.Controls.Add(new Label
        {
            Text = "Search:",
            AutoSize = true,
            Margin = new Padding(8, 14, 0, 0)
        });

        tools.Controls.Add(searchBox);

        searchBox.TextChanged += (_, _) => Refresh();

        Refresh();
    }

    private void ShowReports()
    {
        SetPage("Points && Reports");

        var intro = MakeLabel("Calculate points based on each member's role and event participation.", 10);
        intro.Location = new Point(5, 5);
        intro.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        var tools = new FlowLayoutPanel
        {
            Location = new Point(5, 38),
            Size = new Size(content.ClientSize.Width - 10, 42),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = cream,
            Padding = new Padding(0),
            Margin = new Padding(0),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var attendanceLabel = MakeLabel("Events attended:", 10, true);
        attendanceLabel.Margin = new Padding(5, 10, 5, 5);

        var attendance = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 100,
            Value = 1,
            Width = 75,
            Height = 30,
            Margin = new Padding(5, 5, 10, 5)
        };

        tools.Controls.Add(attendanceLabel);
        tools.Controls.Add(attendance);

        var grid = MakeGrid();
        grid.Location = new Point(5, 90);
        grid.Size = new Size(content.ClientSize.Width - 10, content.ClientSize.Height - 100);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        void CalculateAndBindPoints()
        {
            int attended = (int)attendance.Value;

            grid.DataSource = null;
            grid.DataSource = members
                .Select(m => new
                {
                    m.MemberId,
                    m.FullName,
                    Type = m.MemberType,
                    EventsAttended = attended,
                    Points = m.CalculateEventPoints(attended)
                })
                .OrderByDescending(x => x.Points)
                .ToList();
        }

        tools.Controls.Add(MakeButton("Calculate Points", CalculateAndBindPoints));

        tools.Controls.Add(MakeButton("Save as JSON", () =>
        {
            try
            {
                if (members.Count == 0)
                {
                    MessageBox.Show("No member data available to save.", "Empty Data",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TechGuildEventManager");

                Directory.CreateDirectory(folder);

                string reportPath = Path.Combine(folder, "points_report.json");
                int attended = (int)attendance.Value;

                var reportData = members
                    .Select(m => new
                    {
                        m.MemberId,
                        m.FullName,
                        Type = m.MemberType,
                        EventsAttended = attended,
                        Points = m.CalculateEventPoints(attended)
                    })
                    .OrderByDescending(x => x.Points)
                    .ToList();

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                File.WriteAllText(reportPath, JsonSerializer.Serialize(reportData, jsonOptions));

                SaveAllData();

                MessageBox.Show($"Saved automatically to:\n{reportPath}", "JSON Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save JSON: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }, false));

        content.Controls.Add(intro);
        content.Controls.Add(tools);
        content.Controls.Add(grid);

        CalculateAndBindPoints();
    }

    private void SeedSampleData()
    {
        members.Add(new Officer(1, "Alvedel Bicbic", "alvedelb@gmail.com", "BS Information Technology", 2));
        members.Add(new RegularMember(2, "Keint Ace", "kentoyni@gmail.com", "BS Computer Science", "C#, Java", 1));
        members.Add(new Alumni(3, "Keintbry", "keintbry@gmail.com", "BS Industrial Technology", "Python", 2));

        eventsList.Add(new GuildEvent
        {
            EventId = 4,
            EventName = "Code Camp 2026",
            EventDate = DateTime.Today.AddDays(14),
            Venue = "Computer Laboratory 1",
            Organizer = "Alvedel Bicbic"
        });

        competitions.Add(new TechCompetition
        {
            CompetitionId = 5,
            CompetitionName = "Campus Hackathon",
            ParticipantName = "Keint Ace",
            TechStack = "C# / .NET",
            Rank = 1
        });
    }

    private void LoadAllData()
    {
        try
        {
            _dataService.LoadAllData();

            nextMemberId = members.Count > 0
                ? members.Max(m => m.MemberId) + 1 : 1;

            nextEventId = eventsList.Count > 0
                ? eventsList.Max(e => e.EventId) + 1 : 1;

            nextCompetitionId = competitions.Count > 0
                ? competitions.Max(c => c.CompetitionId) + 1 : 1;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error loading saved JSON data: {ex.Message}",
                "Load Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void SaveAllData()
    {
        try
        {
            _dataService.SaveAllData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error saving JSON data: {ex.Message}",
                "Save Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}

public class MemberDialog : Form
{
    private readonly TextBox txtFullName = new() { Width = 220 };
    private readonly TextBox txtEmail = new() { Width = 220 };
    private readonly TextBox txtCourse = new() { Width = 220 };
    private readonly ComboBox cmbMemberType = new() { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtTechStack = new() { Width = 220 };
    private readonly NumericUpDown numEventsOrganized = new() { Width = 220, Minimum = 0, Maximum = 500 };

    private readonly List<GuildMember> existingMembers;
    private readonly List<GuildEvent> existingEvents;
    private readonly List<TechCompetition> existingCompetitions;
    private readonly int? editingMemberId;
    private int assignedMemberId;

    public MemberDialog(
        List<GuildMember> existingMembers,
        List<GuildEvent> existingEvents,
        List<TechCompetition> existingCompetitions,
        GuildMember? existing = null)
    {
        this.existingMembers = existingMembers;
        this.existingEvents = existingEvents;
        this.existingCompetitions = existingCompetitions;
        editingMemberId = existing?.MemberId;

        Text = existing == null ? "Add Guild Member" : "Edit Guild Member";
        Size = new Size(430, 390);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(247, 242, 235);
        Font = new Font("Segoe UI", 9.5F);

        cmbMemberType.Items.AddRange(new object[] { "Regular Member", "Officer", "Alumni" });
        cmbMemberType.SelectedIndex = 0;

        cmbMemberType.SelectedIndexChanged += (_, _) => UpdateRoleFields();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 2,
            RowCount = 7
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));

        AddRow(layout, 0, "Full Name", txtFullName);
        AddRow(layout, 1, "Email", txtEmail);
        AddRow(layout, 2, "Course", txtCourse);
        AddRow(layout, 3, "Member Type", cmbMemberType);
        AddRow(layout, 4, "Tech Stack", txtTechStack);
        AddRow(layout, 5, "Events Organized", numEventsOrganized);

        var btnSave = new Button
        {
            Text = "Save Member",
            BackColor = Color.FromArgb(130, 91, 63),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Size = new Size(135, 36),
            Margin = new Padding(0, 12, 0, 0),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => ValidateAndSave();

        layout.Controls.Add(new Label(), 0, 6);
        layout.Controls.Add(btnSave, 1, 6);
        Controls.Add(layout);

        if (existing != null)
        {
            txtFullName.Text = existing.FullName;
            txtEmail.Text = existing.Email;
            txtCourse.Text = existing.Course;
            cmbMemberType.SelectedItem = existing.MemberType;

            if (existing is Officer off)
                numEventsOrganized.Value = off.EventsOrganizedCount;
            else if (existing is RegularMember reg)
                txtTechStack.Text = reg.TechStack;
            else if (existing is Alumni alu)
                txtTechStack.Text = alu.TechStack;
        }

        UpdateRoleFields();
    }

   private void UpdateRoleFields()
    {
        bool isOfficer = cmbMemberType.SelectedItem?.ToString() == "Officer";
        numEventsOrganized.Enabled = true; // Accessible for all member types
        txtTechStack.Enabled = !isOfficer;

        if (isOfficer) txtTechStack.Clear();
    }

    private static string Normalize(string value) =>
        new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLowerInvariant();

    private int GetNextUniqueGlobalId()
    {
        int maxId = new[]
        {
            existingMembers.Count > 0 ? existingMembers.Max(m => m.MemberId) : 0,
            existingEvents.Count > 0 ? existingEvents.Max(e => e.EventId) : 0,
            existingCompetitions.Count > 0 ? existingCompetitions.Max(c => c.CompetitionId) : 0
        }.Max();

        return maxId + 1;
    }

    private bool IsIdAlreadyInUse(int id, int? ignoreMemberId = null) =>
        existingMembers.Any(m => m.MemberId != ignoreMemberId && m.MemberId == id) ||
        existingEvents.Any(e => e.EventId == id) ||
        existingCompetitions.Any(c => c.CompetitionId == id);

    private void ValidateAndSave()
    {
        string fullName = txtFullName.Text.Trim();
        string email = txtEmail.Text.Trim();
        string course = txtCourse.Text.Trim();
        string memberType = cmbMemberType.SelectedItem?.ToString() ?? "";
        string techStack = txtTechStack.Text.Trim();

        if (string.IsNullOrWhiteSpace(fullName) || fullName.All(char.IsDigit))
        {
            MessageBox.Show("Please enter a valid Full Name (letters required).", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtFullName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
        {
            MessageBox.Show("Please enter a valid Email address (e.g., name@domain.com).", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtEmail.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(course) || course.All(char.IsDigit))
        {
            MessageBox.Show("Please enter a valid Course (e.g., BSIT, BSCS).", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCourse.Focus();
            return;
        }

        if ((memberType == "Regular Member" || memberType == "Alumni") && string.IsNullOrWhiteSpace(techStack))
        {
            MessageBox.Show($"{memberType}s must specify at least one Tech Stack.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtTechStack.Focus();
            return;
        }

        bool duplicateName = existingMembers.Any(m =>
            m.MemberId != editingMemberId &&
            Normalize(m.FullName) == Normalize(fullName));

        if (duplicateName)
        {
            MessageBox.Show("Full Name already exists. Cannot save.",
                "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtFullName.Focus();
            return;
        }

        bool duplicateEmail = existingMembers.Any(m =>
            m.MemberId != editingMemberId &&
            m.Email.Trim().Equals(email, StringComparison.OrdinalIgnoreCase));

        if (duplicateEmail)
        {
            MessageBox.Show("Email already exists. Cannot save.",
                "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtEmail.Focus();
            return;
        }

        int candidateId = editingMemberId ?? GetNextUniqueGlobalId();
        if (IsIdAlreadyInUse(candidateId, editingMemberId))
        {
            MessageBox.Show($"ID {candidateId} already exists. Cannot save.",
                "ID Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        assignedMemberId = candidateId;
        DialogResult = DialogResult.OK;
        Close();
    }

    public GuildMember CreateMember(int fallbackId = 0)
    {
        int id = assignedMemberId > 0 ? assignedMemberId : (editingMemberId ?? GetNextUniqueGlobalId());
        string name = txtFullName.Text.Trim();
        string email = txtEmail.Text.Trim();
        string course = txtCourse.Text.Trim();
        string type = cmbMemberType.SelectedItem?.ToString() ?? "Regular Member";

        return type switch
        {
            "Officer" => new Officer(id, name, email, course, (int)numEventsOrganized.Value),
            "Alumni" => new Alumni(id, name, email, course, txtTechStack.Text.Trim()),
            _ => new RegularMember(id, name, email, course, txtTechStack.Text.Trim())
        };
    }

    private static void AddRow(TableLayoutPanel panel, int row, string labelText, Control input)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.Controls.Add(new Label
        {
            Text = labelText,
            ForeColor = Color.FromArgb(78, 52, 36),
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill
        }, 0, row);
        panel.Controls.Add(input, 1, row);
    }
}

public class EventDialog : Form
{
    private readonly TextBox txtEventName = new() { Width = 220 };
    private readonly DateTimePicker dtpEventDate = new() { Width = 220, Format = DateTimePickerFormat.Short };
    private readonly TextBox txtVenue = new() { Width = 220 };
    private readonly TextBox txtOrganizer = new() { Width = 220 };

    private readonly List<GuildMember> existingMembers;
    private readonly List<GuildEvent> existingEvents;
    private readonly List<TechCompetition> existingCompetitions;
    private readonly int? editingEventId;
    private int assignedEventId;

    public EventDialog(
        List<GuildMember> existingMembers,
        List<GuildEvent> existingEvents,
        List<TechCompetition> existingCompetitions,
        GuildEvent? existing = null)
    {
        this.existingMembers = existingMembers;
        this.existingEvents = existingEvents;
        this.existingCompetitions = existingCompetitions;
        editingEventId = existing?.EventId;

        Text = existing == null ? "Add Guild Event" : "Edit Guild Event";
        Size = new Size(430, 310);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(247, 242, 235);
        Font = new Font("Segoe UI", 9.5F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 2,
            RowCount = 5
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));

        AddRow(layout, 0, "Event Name", txtEventName);
        AddRow(layout, 1, "Event Date", dtpEventDate);
        AddRow(layout, 2, "Venue", txtVenue);
        AddRow(layout, 3, "Organizer", txtOrganizer);

        var btnSave = new Button
        {
            Text = "Save Event",
            BackColor = Color.FromArgb(130, 91, 63),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Size = new Size(135, 36),
            Margin = new Padding(0, 12, 0, 0),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) =>
        {
            string eventName = txtEventName.Text.Trim();
            string venue = txtVenue.Text.Trim();
            string organizer = txtOrganizer.Text.Trim();
            DateTime date = dtpEventDate.Value.Date;

            if (string.IsNullOrWhiteSpace(eventName))
            {
                MessageBox.Show("Event Name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEventName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(venue))
            {
                MessageBox.Show("Venue is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtVenue.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(organizer) || organizer.All(char.IsDigit))
            {
                MessageBox.Show("Please enter a valid Organizer name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOrganizer.Focus();
                return;
            }

            bool duplicateEvent = existingEvents.Any(e =>
                e.EventId != editingEventId &&
                e.EventName.Trim().Equals(eventName, StringComparison.OrdinalIgnoreCase) &&
                e.EventDate.Date == date);

            if (duplicateEvent)
            {
                MessageBox.Show("Event already exists. Cannot save.",
                    "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEventName.Focus();
                return;
            }

            int candidateId = editingEventId ?? GetNextUniqueGlobalId();
            if (IsIdAlreadyInUse(candidateId, editingEventId))
            {
                MessageBox.Show($"ID {candidateId} already exists. Cannot save.",
                    "ID Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            assignedEventId = candidateId;
            DialogResult = DialogResult.OK;
            Close();
        };

        layout.Controls.Add(new Label(), 0, 4);
        layout.Controls.Add(btnSave, 1, 4);
        Controls.Add(layout);

        if (existing != null)
        {
            txtEventName.Text = existing.EventName;
            dtpEventDate.Value = existing.EventDate;
            txtVenue.Text = existing.Venue;
            txtOrganizer.Text = existing.Organizer;
        }
    }

    private int GetNextUniqueGlobalId()
    {
        int maxId = new[]
        {
            existingMembers.Count > 0 ? existingMembers.Max(m => m.MemberId) : 0,
            existingEvents.Count > 0 ? existingEvents.Max(e => e.EventId) : 0,
            existingCompetitions.Count > 0 ? existingCompetitions.Max(c => c.CompetitionId) : 0
        }.Max();

        return maxId + 1;
    }

    private bool IsIdAlreadyInUse(int id, int? ignoreEventId = null) =>
        existingMembers.Any(m => m.MemberId == id) ||
        existingEvents.Any(e => e.EventId != ignoreEventId && e.EventId == id) ||
        existingCompetitions.Any(c => c.CompetitionId == id);

    public GuildEvent CreateEvent(int fallbackId = 0) => new()
    {
        EventId = assignedEventId > 0 ? assignedEventId : (editingEventId ?? GetNextUniqueGlobalId()),
        EventName = txtEventName.Text.Trim(),
        EventDate = dtpEventDate.Value.Date,
        Venue = txtVenue.Text.Trim(),
        Organizer = txtOrganizer.Text.Trim()
    };

    private static void AddRow(TableLayoutPanel panel, int row, string labelText, Control input)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.Controls.Add(new Label
        {
            Text = labelText,
            ForeColor = Color.FromArgb(78, 52, 36),
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill
        }, 0, row);
        panel.Controls.Add(input, 1, row);
    }
}

public class CompetitionDialog : Form
{
    private readonly TextBox txtCompetitionName = new() { Width = 220 };
    private readonly TextBox txtParticipantName = new() { Width = 220 };
    private readonly TextBox txtTechStack = new() { Width = 220 };
    private readonly NumericUpDown numRank = new() { Width = 220, Minimum = 1, Maximum = 100, Value = 1 };

    private readonly List<GuildMember> existingMembers;
    private readonly List<GuildEvent> existingEvents;
    private readonly List<TechCompetition> existingCompetitions;
    private readonly int? editingCompetitionId;
    private int assignedCompetitionId;

    public CompetitionDialog(
        List<GuildMember> existingMembers,
        List<GuildEvent> existingEvents,
        List<TechCompetition> existingCompetitions,
        TechCompetition? existing = null)
    {
        this.existingMembers = existingMembers;
        this.existingEvents = existingEvents;
        this.existingCompetitions = existingCompetitions;
        editingCompetitionId = existing?.CompetitionId;

        Text = existing == null ? "Add Competition" : "Edit Competition";
        Size = new Size(430, 310);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(247, 242, 235);
        Font = new Font("Segoe UI", 9.5F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 2,
            RowCount = 5
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));

        AddRow(layout, 0, "Competition Name", txtCompetitionName);
        AddRow(layout, 1, "Participant Name", txtParticipantName);
        AddRow(layout, 2, "Tech Stack", txtTechStack);
        AddRow(layout, 3, "Rank", numRank);

        var btnSave = new Button
        {
            Text = "Save Competition",
            BackColor = Color.FromArgb(130, 91, 63),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Size = new Size(145, 36),
            Margin = new Padding(0, 12, 0, 0),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) =>
        {
            string compName = txtCompetitionName.Text.Trim();
            string participant = txtParticipantName.Text.Trim();
            string techStack = txtTechStack.Text.Trim();

            if (string.IsNullOrWhiteSpace(compName))
            {
                MessageBox.Show("Competition Name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCompetitionName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(participant) || participant.All(char.IsDigit))
            {
                MessageBox.Show("Please enter a valid Participant Name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtParticipantName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(techStack))
            {
                MessageBox.Show("Tech Stack is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTechStack.Focus();
                return;
            }

            bool duplicateComp = existingCompetitions.Any(c =>
                c.CompetitionId != editingCompetitionId &&
                c.CompetitionName.Trim().Equals(compName, StringComparison.OrdinalIgnoreCase) &&
                c.ParticipantName.Trim().Equals(participant, StringComparison.OrdinalIgnoreCase));

            if (duplicateComp)
            {
                MessageBox.Show("Competition entry already exists. Cannot save.",
                    "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtParticipantName.Focus();
                return;
            }

            int candidateId = editingCompetitionId ?? GetNextUniqueGlobalId();
            if (IsIdAlreadyInUse(candidateId, editingCompetitionId))
            {
                MessageBox.Show($"ID {candidateId} already exists. Cannot save.",
                    "ID Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            assignedCompetitionId = candidateId;
            DialogResult = DialogResult.OK;
            Close();
        };

        layout.Controls.Add(new Label(), 0, 4);
        layout.Controls.Add(btnSave, 1, 4);
        Controls.Add(layout);

        if (existing != null)
        {
            txtCompetitionName.Text = existing.CompetitionName;
            txtParticipantName.Text = existing.ParticipantName;
            txtTechStack.Text = existing.TechStack;
            numRank.Value = Math.Max(1, existing.Rank);
        }
    }

    private int GetNextUniqueGlobalId()
    {
        int maxId = new[]
        {
            existingMembers.Count > 0 ? existingMembers.Max(m => m.MemberId) : 0,
            existingEvents.Count > 0 ? existingEvents.Max(e => e.EventId) : 0,
            existingCompetitions.Count > 0 ? existingCompetitions.Max(c => c.CompetitionId) : 0
        }.Max();

        return maxId + 1;
    }

    private bool IsIdAlreadyInUse(int id, int? ignoreCompetitionId = null) =>
        existingMembers.Any(m => m.MemberId == id) ||
        existingEvents.Any(e => e.EventId == id) ||
        existingCompetitions.Any(c => c.CompetitionId != ignoreCompetitionId && c.CompetitionId == id);

    public TechCompetition CreateCompetition(int fallbackId = 0) => new()
    {
        CompetitionId = assignedCompetitionId > 0 ? assignedCompetitionId : (editingCompetitionId ?? GetNextUniqueGlobalId()),
        CompetitionName = txtCompetitionName.Text.Trim(),
        ParticipantName = txtParticipantName.Text.Trim(),
        TechStack = txtTechStack.Text.Trim(),
        Rank = (int)numRank.Value
    };

    private static void AddRow(TableLayoutPanel panel, int row, string labelText, Control input)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.Controls.Add(new Label
        {
            Text = labelText,
            ForeColor = Color.FromArgb(78, 52, 36),
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill
        }, 0, row);
        panel.Controls.Add(input, 1, row);
    }
}