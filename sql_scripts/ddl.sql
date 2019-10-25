SET FOREIGN_KEY_CHECKS=0;

--
-- Table structure for table `AuthTokens`
--

DROP TABLE IF EXISTS `AuthTokens`;
CREATE TABLE `AuthTokens` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `type` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `token` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `Channels`
--

DROP TABLE IF EXISTS `Channels`;
CREATE TABLE `Channels` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `channel_id` bigint(20) unsigned NOT NULL,
  `channel_type` int(11) NOT NULL,
  `profanity_check_exempt` tinyint(4) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=52 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `FAQCommandChannelEntry`
--

DROP TABLE IF EXISTS `FAQCommandChannelEntry`;
CREATE TABLE `FAQCommandChannelEntry` (
  `entry_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `text` mediumtext COLLATE utf8mb4_unicode_ci,
  `is_embed` tinyint(4) NOT NULL,
  `image_url` text COLLATE utf8mb4_unicode_ci,
  `hex_color` int(11) unsigned NOT NULL,
  `author` text COLLATE utf8mb4_unicode_ci,
  `author_avatar_url` text COLLATE utf8mb4_unicode_ci,
  `position` int(11) unsigned NOT NULL,
  `changed_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `command_channel_id_reference` int(10) unsigned NOT NULL,
  PRIMARY KEY (`entry_id`),
  KEY `fk_command_channel` (`command_channel_id_reference`),
  CONSTRAINT `fk_command_channel` FOREIGN KEY (`command_channel_id_reference`) REFERENCES `FaqCommandChannel` (`command_channel_id`)
) ENGINE=InnoDB AUTO_INCREMENT=359 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `FaqCommand`
--

DROP TABLE IF EXISTS `FaqCommand`;
CREATE TABLE `FaqCommand` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `aliases` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=40 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `FaqCommandChannel`
--

DROP TABLE IF EXISTS `FaqCommandChannel`;
CREATE TABLE `FaqCommandChannel` (
  `command_channel_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `command_id` int(10) unsigned NOT NULL,
  `channel_id` int(10) unsigned NOT NULL,
  PRIMARY KEY (`command_channel_id`),
  KEY `fk_channel_id` (`channel_id`),
  KEY `command_id` (`command_id`),
  CONSTRAINT `fk_channel_id` FOREIGN KEY (`channel_id`) REFERENCES `Channels` (`id`),
  CONSTRAINT `fk_command_id` FOREIGN KEY (`command_id`) REFERENCES `FaqCommand` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=359 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `Mutes`
--

DROP TABLE IF EXISTS `Mutes`;
CREATE TABLE `Mutes` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `muted_user` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `muted_user_id` bigint(20) unsigned NOT NULL,
  `muted_by` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `muted_by_id` bigint(20) unsigned NOT NULL,
  `reason` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `mute_date` datetime NOT NULL,
  `unmute_date` datetime NOT NULL,
  `mute_ended` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=82 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `PersistentData`
--

DROP TABLE IF EXISTS `PersistentData`;
CREATE TABLE `PersistentData` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  `val` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=latin1;

--
-- Table structure for table `ProfanityChecks`
--

DROP TABLE IF EXISTS `ProfanityChecks`;
CREATE TABLE `ProfanityChecks` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `regex` text COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `ReferralCodes`
--

DROP TABLE IF EXISTS `ReferralCodes`;
CREATE TABLE `ReferralCodes` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `sender` bigint(20) unsigned NOT NULL,
  `code` text NOT NULL,
  `date` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;


--
-- Table structure for table `Roles`
--

DROP TABLE IF EXISTS `Roles`;
CREATE TABLE `Roles` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  `role_id` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=latin1;

--
-- Table structure for table `StarboardMessages`
--

DROP TABLE IF EXISTS `StarboardMessages`;
CREATE TABLE `StarboardMessages` (
  `message_id` bigint(20) unsigned NOT NULL,
  `starboard_message_id` bigint(20) unsigned NOT NULL,
  `star_count` int(10) unsigned NOT NULL,
  `author_id` bigint(20) unsigned NOT NULL,
  `ignored` tinyint(1) NOT NULL,
  PRIMARY KEY (`message_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `StarboardPostRelations`
--

DROP TABLE IF EXISTS `StarboardPostRelations`;
CREATE TABLE `StarboardPostRelations` (
  `user_id` bigint(20) unsigned NOT NULL,
  `message_id` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`user_id`,`message_id`),
  KEY `fk_starboardpost_id` (`message_id`),
  CONSTRAINT `fk_starboardpost_id` FOREIGN KEY (`message_id`) REFERENCES `StarboardMessages` (`message_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `Warnings`
--

DROP TABLE IF EXISTS `Warnings`;
CREATE TABLE `Warnings` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `warned_user` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `warned_user_id` bigint(20) unsigned NOT NULL,
  `warned_by` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `warned_by_id` bigint(20) unsigned NOT NULL,
  `reason` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `date` datetime NOT NULL,
  `decayed` tinyint(4) NOT NULL,
  `decayed_date` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `Reminders`
--

DROP TABLE IF EXISTS `Reminders`;
CREATE TABLE `Reminders` (
 `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
 `reminded_user_id` bigint(20) unsigned NOT NULL,
 `remind_text` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
 `reminder_date` datetime NOT NULL,
 `reminded` tinyint(1) NOT NULL DEFAULT '1',
 `channel_id` bigint(20) unsigned NOT NULL,
 `target_date` datetime NOT NULL,
 `message_id` bigint(20) unsigned NOT NULL,
 PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `InviteLinks`
--

DROP TABLE IF EXISTS `InviteLinks`;
CREATE TABLE `InviteLinks` (
 `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
 `link` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
 `label` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
 `added_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
 PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `ModMailThread`
--

DROP TABLE IF EXISTS `ModMailThread`;
CREATE TABLE `ModMailThread` (
 `user_id` bigint(20) unsigned NOT NULL,
 `channel_id` bigint(20) unsigned NOT NULL,
 `created_date` datetime NOT NULL,
 `update_date` datetime NOT NULL,
 `closed_date` datetime NOT NULL,
 `state` varchar(25) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'INITIAL',
 PRIMARY KEY (`channel_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `ThreadSubscribers`
--

DROP TABLE IF EXISTS `ThreadSubscribers`;
CREATE TABLE `ThreadSubscribers` (
 `channel_id` bigint(20) unsigned NOT NULL,
 `user_id` bigint(20) unsigned NOT NULL,
 PRIMARY KEY (`channel_id`,`user_id`),
 CONSTRAINT `fk_thread_ref` FOREIGN KEY (`channel_id`) REFERENCES `ModMailThread` (`channel_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `ThreadMessage`
--


DROP TABLE IF EXISTS `ThreadMessage`;
CREATE TABLE `ThreadMessage` (
 `channel_id` bigint(20) unsigned NOT NULL,
 `channel_message_id` bigint(20) unsigned NOT NULL,
 `user_message_id` bigint(20) unsigned NOT NULL,
 `user_id` bigint(20) unsigned NOT NULL,
 `anonymous` tinyint(4) NOT NULL,
 PRIMARY KEY (`channel_id`,`channel_message_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SET FOREIGN_KEY_CHECKS=1;